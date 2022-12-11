using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Concurrent;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Services
{
    internal sealed class NavigationService<TNavigationKey> : IMultiLevelNavigationService<TNavigationKey> where TNavigationKey : Enum
    {
        private readonly IPageLocatorService<TNavigationKey> _pageLocator;
        private readonly ConcurrentStack<Frame> _frameStack;
        private Frame _currentFrame;

        public NavigationService(IPageLocatorService<TNavigationKey> pageLocator)
        {
            _frameStack = new ConcurrentStack<Frame>();
            _pageLocator = pageLocator;
        }

        public int NavigationDepth => _frameStack.Count;

        public bool CanGoBack => _currentFrame?.CanGoBack ?? false;

        public bool CanGoForward => _currentFrame?.CanGoForward ?? false;

        public void AddNavigationLevel(Frame frame)
        {
            if (frame == default)
            {
                throw new ArgumentNullException(nameof(frame));
            }
            if (_currentFrame == default || _currentFrame.GetHashCode() != frame.GetHashCode())
            {
                if (_frameStack.TryPeek(out var lastFrame) && lastFrame.GetHashCode() != _currentFrame.GetHashCode())
                {
                    _currentFrame.Navigated -= ActiveFrameNavigated;
                    _frameStack.Push(_currentFrame);
                }
                _currentFrame = frame;
                _currentFrame.Navigated += ActiveFrameNavigated;
            }
            else
            {
                throw new Exception("Frame is already in the navigation stack");
            }
        }

        private void ActiveFrameNavigated(object sender, NavigationEventArgs e)
        {
            if (sender is not Frame frame)
            {
                return;
            }
            DispatchNavigatedTo(frame, e.Parameter);
        }

        public void RemoveNavigationLevel(Frame frame)
        {
            if (frame == default)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            if (NavigationDepth >= 1)
            {
                if (_currentFrame.GetHashCode() == frame.GetHashCode())
                {
                    _currentFrame.Navigated -= ActiveFrameNavigated;
                    if (_frameStack.TryPop(out _currentFrame))
                    {
                        _currentFrame.Navigated += ActiveFrameNavigated;
                    }
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
            if (_currentFrame == default)
            {
                throw new ArgumentNullException(nameof(_currentFrame));
            }
            _currentFrame.GoBack();
        }

        public void Navigate(TNavigationKey key, object args, NavigationStackMode navigationStackMode)
        {
            if (_currentFrame == default)
            {
                throw new ArgumentException(nameof(_currentFrame));
            }

            DispatchNavigatedFrom(_currentFrame);
            var pageType = _pageLocator.GetPageTypeForKey(key);
            _currentFrame.Navigate(pageType, args);

            if (navigationStackMode == NavigationStackMode.Skip)
            {
                RemoveLastNavigationStackItem(_currentFrame);
            }

            if (navigationStackMode == NavigationStackMode.Clear)
            {
                ClearNavigationStack(_currentFrame);
            }
        }

        public void Navigate(TNavigationKey key, NavigationTransitionInfo transitionInfo, object args, NavigationStackMode navigationStackMode)
        {
            if (_currentFrame == default)
            {
                throw new ArgumentException(nameof(_currentFrame));
            }

            DispatchNavigatedFrom(_currentFrame);
            var pageType = _pageLocator.GetPageTypeForKey(key);
            _currentFrame.Navigate(pageType, args, transitionInfo);

            if (navigationStackMode == NavigationStackMode.Skip)
            {
                RemoveLastNavigationStackItem(_currentFrame);
            }

            if (navigationStackMode == NavigationStackMode.Clear)
            {
                ClearNavigationStack(_currentFrame);
            }
        }

        public void ClearNavigationStack()
        {
            _frameStack.Clear();
        }

        private static void DispatchNavigatedFrom(Frame frame)
        {
            if (frame.Content is not Page targetPage)
            {
                return;
            }
            if (targetPage.DataContext is not INavigationAware navigationAware)
            {
                return;
            }
            navigationAware.OnNavigatedFrom();
        }

        private static void DispatchNavigatedTo(Frame frame, object parameter)
        {
            if (frame.Content is not Page targetPage)
            {
                return;
            }
            if (targetPage.DataContext is not INavigationAware navigationAware)
            {
                return;
            }
            navigationAware.OnNavigatedTo(parameter);
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
