using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Concurrent;
using System.Linq;
using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Services
{
    public enum NavigationStackMode
    {
        Add,
        Skip,
        Clear,
    }

    public interface INavigationService<TNavigationKey> where TNavigationKey : Enum
    {
        public void GoBack();
        public void Navigate(TNavigationKey key, object args, NavigationStackMode navigationStackMode = NavigationStackMode.Add);
        public void Navigate(TNavigationKey key, NavigationTransitionInfo transitionInfo,
            object args, NavigationStackMode navigationStackMode = NavigationStackMode.Add);
    }

    public interface IMultiLevelNavigationService<TNavigationKey> : INavigationService<TNavigationKey> where TNavigationKey : Enum
    {
        public void AddNavigationLevel(Frame containerFrame);
        public void RemoveNavigationLevel(Frame containerFrame);
        public void ClearNavigationStack();
    }


    internal class NavigationService<TNavigationKey> : IMultiLevelNavigationService<TNavigationKey> where TNavigationKey : Enum
    {
        private readonly PageLocatorService<TNavigationKey> _pageLocator;
        private readonly ConcurrentStack<Frame> _frameStack;

        public NavigationService(PageLocatorService<TNavigationKey> pageLocator)
        {
            _frameStack = new ConcurrentStack<Frame>();
            _pageLocator = pageLocator;
        }

        public int NavigationDepth => _frameStack.Count;

        private Frame CurrentFrame => PeekFrameStack();

        public bool CanGoBack => CurrentFrame?.CanGoBack ?? false;

        public bool CanGoForward => CurrentFrame?.CanGoForward ?? false;

        public void AddNavigationLevel(Frame frame)
        {
            if (frame == default)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            if (!_frameStack.Any(f => f.GetHashCode() == frame.GetHashCode()))
            {
                _frameStack.Push(frame);
            }
            else
            {
                throw new Exception("Frame is already in the navigation stack");
            }
        }

        public void RemoveNavigationLevel(Frame frame)
        {
            if (frame == default)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            if (NavigationDepth >= 1)
            {
                if (CurrentFrame.GetHashCode() == frame.GetHashCode())
                {
                    _ = _frameStack.TryPop(out var _);
                }
                else
                {
                    throw new Exception("Trying to remove a different frame from the stack");
                }
            }
            else
            {
                throw new Exception("No Navigation Level can be removed");
            }
        }

        public void GoBack()
        {
            if (CurrentFrame == default)
            {
                throw new ArgumentNullException(nameof(CurrentFrame));
            }
            CurrentFrame.GoBack();
        }

        public void Navigate(TNavigationKey key, object args, NavigationStackMode navigationStackMode)
        {
            if (CurrentFrame == default)
            {
                throw new ArgumentNullException(nameof(CurrentFrame));
            }

            var pageType = _pageLocator.GetPageTypeForKey(key);
            CurrentFrame.Navigate(pageType, args);

            if (navigationStackMode == NavigationStackMode.Skip)
            {
                RemoveLastNavigationStackItem(CurrentFrame);
            }

            if (navigationStackMode == NavigationStackMode.Clear)
            {
                ClearNavigationStack(CurrentFrame);
            }
        }

        public void Navigate(TNavigationKey key, NavigationTransitionInfo transitionInfo, object args, NavigationStackMode navigationStackMode)
        {
            if (CurrentFrame == default)
            {
                throw new ArgumentNullException(nameof(CurrentFrame));
            }

            var pageType = _pageLocator.GetPageTypeForKey(key);
            CurrentFrame.Navigate(pageType, args, transitionInfo);

            if (navigationStackMode == NavigationStackMode.Skip)
            {
                RemoveLastNavigationStackItem(CurrentFrame);
            }

            if (navigationStackMode == NavigationStackMode.Clear)
            {
                ClearNavigationStack(CurrentFrame);
            }
        }

        public void ClearNavigationStack()
        {
            _frameStack.Clear();
        }

        private Frame PeekFrameStack()
        {
            if (_frameStack.TryPeek(out var frame))
            {
                return frame;
            }
            throw new Exception("Failed to Peek last stack frame");
        }

        private static void RemoveLastNavigationStackItem(Frame frame)
        {
            if (frame.BackStackDepth > 0)
            {
                frame.BackStack.RemoveAt(frame.BackStack.Count - 1);
            }
        }

        private static void ClearNavigationStack(Frame frame)
        {
            if (frame.BackStackDepth > 0)
            {
                frame.BackStack.Clear();
            }
        }
    }
}
