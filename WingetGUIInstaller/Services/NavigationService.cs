using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Concurrent;
using System.Linq;
using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Services
{
    public interface INavigationService<TNavigationKey> where TNavigationKey : Enum
    {
        public void GoBack();
        public void Navigate(TNavigationKey key, object args);
        public void Navigate(TNavigationKey key, NavigationTransitionInfo transitionInfo, object args);
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
            CurrentFrame?.GoBack();
        }

        public void Navigate(TNavigationKey key, object args)
        {
            var pageType = _pageLocator.GetPageTypeForKey(key);
            CurrentFrame?.Navigate(pageType, args);
        }

        public void Navigate(TNavigationKey key, NavigationTransitionInfo transitionInfo, object args)
        {
            var pageType = _pageLocator.GetPageTypeForKey(key);
            CurrentFrame?.Navigate(pageType, args, transitionInfo);
        }

        private Frame PeekFrameStack()
        {
            if (_frameStack.TryPeek(out var frame))
            {
                return frame;
            }
            throw new Exception("Failed to Peek last stack frame");
        }

        public void ClearNavigationStack()
        {
            _frameStack.Clear();
        }
    }
}
