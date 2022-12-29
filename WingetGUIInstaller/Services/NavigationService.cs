using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Concurrent;
using System.Security.AccessControl;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Services
{
    internal sealed class NavigationService<TNavigationKey> : IMultiLevelNavigationService<TNavigationKey> where TNavigationKey : Enum
    {
        private readonly IPageLocatorService<TNavigationKey> _pageLocator;
        private readonly ConcurrentStack<Frame> _frameStack;
        private Frame _currentFrame;
        private readonly ILogger _logger;

        public NavigationService(IPageLocatorService<TNavigationKey> pageLocator, ILogger<NavigationService<NavigationItemKey>> logger)
        {
            _frameStack = new ConcurrentStack<Frame>();
            _pageLocator = pageLocator;
            _logger = logger;
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
                if (_currentFrame != default && _frameStack.TryPeek(out var lastFrame) &&
                    lastFrame.GetHashCode() != _currentFrame.GetHashCode())
                {
                    _currentFrame.Navigated -= ActiveFrameNavigated;
                    _currentFrame.Navigating -= ActiveFrameNavigating;
                    _frameStack.Push(_currentFrame);
                    _logger.LogDebug("Added new level to navigation stack");
                }
                _currentFrame = frame;
                _currentFrame.Navigated += ActiveFrameNavigated;
                _currentFrame.Navigating += ActiveFrameNavigating;
            }
            else
            {
                _logger.LogError("Frame is already in the navigation stack");
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

        private void ActiveFrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (sender is not Frame frame)
            {
                return;
            }
            DispatchNavigatedFrom(frame, e.NavigationMode);
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
                    _logger.LogDebug("Removing a level from navigation stack");
                    _currentFrame.Navigated -= ActiveFrameNavigated;
                    _currentFrame.Navigating -= ActiveFrameNavigating;
                    if (_frameStack.TryPop(out _currentFrame))
                    {
                        _currentFrame.Navigated += ActiveFrameNavigated;
                        _currentFrame.Navigating += ActiveFrameNavigating;
                    }
                }
                else
                {
                    _logger.LogError("Trying to remove a different frame from navigation stack");
                    throw new Exception("Trying to remove a different frame from navigation stack");
                }
            }
            else
            {
                _logger.LogWarning("Frame stack is empty");
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

        public void GoForward()
        {
            if (_currentFrame == default)
            {
                throw new ArgumentNullException(nameof(_currentFrame));
            }
            _currentFrame.GoForward();
        }

        public void Navigate(TNavigationKey key, NavigationTransitionInfo transitionInfo = default, object args = default, NavigationStackMode navigationStackMode = NavigationStackMode.Add)
        {
            if (_currentFrame == default)
            {
                throw new ArgumentException(nameof(_currentFrame));
            }

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

        private void DispatchNavigatedFrom(Frame frame, NavigationMode navigationMode = NavigationMode.New)
        {
            if (frame.Content is not Page sourcePage)
            {
                return;
            }
            _logger.LogDebug("Navigating from: {page} Mode: {mode}", sourcePage.GetType().Name, navigationMode);
            if (sourcePage.DataContext is not INavigationAware navigationAware)
            {
                return;
            }
            navigationAware.OnNavigatedFrom(navigationMode);
        }

        private void DispatchNavigatedTo(Frame frame, object parameter)
        {
            if (frame.Content is not Page targetPage)
            {
                return;
            }
            _logger.LogDebug("Navigating to: {page}", targetPage.GetType().Name);
            if (targetPage.DataContext is not INavigationAware navigationAware)
            {
                return;
            }
            navigationAware.OnNavigatedTo(parameter);
        }

        private void RemoveLastNavigationStackItem(Frame frame)
        {
            if (frame.BackStackDepth > 0)
            {
                frame.BackStack.RemoveAt(frame.BackStack.Count - 1);
                _logger.LogDebug("Removed the last navigation stack item");
            }
        }

        private void ClearNavigationStack(Frame frame)
        {
            if (frame.BackStackDepth > 0)
            {
                frame.BackStack.Clear();
                _logger.LogDebug("Navigation stack was cleared");
            }
        }
    }
}
