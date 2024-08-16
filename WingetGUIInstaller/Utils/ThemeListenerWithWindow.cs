using Microsoft.UI.Xaml;
using Microsoft.Win32;
using System;
using System.Threading;

namespace WingetGUIInstaller.Utils
{
    internal sealed class ThemeListenerWithWindow : IDisposable
    {
        private readonly TimeSpan _eventInterval = TimeSpan.FromMilliseconds(500);
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Thread _watchthread;
        private readonly Window _window;

        private DateTimeOffset _lastUpdatedTime;
        private ApplicationTheme _currentTheme;

        public delegate void ThemeChangedEventWithWindow(ThemeListenerWithWindow sender);

        public event ThemeChangedEventWithWindow ThemeChanged;

        public ApplicationTheme CurrentTheme => _currentTheme;

        public ThemeListenerWithWindow(Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _currentTheme = WindowInteropUtils.GetUserThemePreference();
            _lastUpdatedTime = DateTimeOffset.MinValue;
            _cancellationTokenSource = new CancellationTokenSource();
            _watchthread = new Thread(Watch);
            _watchthread.Start();
        }

        private void Watch()
        {
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }
                Thread.Sleep(500);
            }
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                if (_lastUpdatedTime.Add(_eventInterval) >= DateTimeOffset.UtcNow)
                {
                    return;
                }
                _lastUpdatedTime = DateTimeOffset.UtcNow;
                var newTheme = WindowInteropUtils.GetUserThemePreference();
                if (newTheme != _currentTheme)
                {
                    _currentTheme = newTheme;
                    _window.DispatcherQueue.TryEnqueue(() => ThemeChanged(this));
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _watchthread.Join();
            _cancellationTokenSource.Dispose();
        }
    }
}
