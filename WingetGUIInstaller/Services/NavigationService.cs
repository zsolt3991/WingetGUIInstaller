using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        public NavigationService(PageLocatorService<TNavigationKey> pageLocator, ILogger<NavigationService<NavigationItemKey>> logger)
        {
            _frameStack = new ConcurrentStack<Frame>();
            _pageLocator = pageLocator;
            _logger = logger;
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
                _logger.LogError("Frame is already in the navigation stack");
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
                    _logger.LogError("Trying to remove a different frame from the stack");
                    throw new Exception("Trying to remove a different frame from the stack");
                }
            }
            else
            {
                _logger.LogWarning("Frame stack is empty");
            }
        }

        public void GoBack()
        {
            CurrentFrame?.GoBack();
        }

        public void Navigate(TNavigationKey key, object args)
        {
            _logger.LogDebug("Navigating to key: {key}", key);
            var pageType = _pageLocator.GetPageTypeForKey(key);
            CurrentFrame?.Navigate(pageType, args);
        }

        public void Navigate(TNavigationKey key, NavigationTransitionInfo transitionInfo, object args)
        {
            _logger.LogDebug("Navigating to key: {key}", key);
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
            _logger.LogDebug("Navigation stack was cleared");
        }
    }
}
