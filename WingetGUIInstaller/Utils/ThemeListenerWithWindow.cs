using Microsoft.UI.Xaml;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WingetGUIInstaller.Utils
{
    internal class ThemeListenerWithWindow : IDisposable
    {
        private readonly CancellationTokenSource _cancellationSource;
        private readonly Task _wathcerTask;
        private ApplicationTheme _currentTheme;

        public delegate void ThemeChangedEventWithWindow(ThemeListenerWithWindow sender);

        public event ThemeChangedEventWithWindow ThemeChanged;

        public ApplicationTheme CurrentTheme => _currentTheme;

        public ThemeListenerWithWindow(Window window)
        {
            _currentTheme = WindowInteropUtils.GetUserThemePreference();
            _cancellationSource = new CancellationTokenSource();
            _wathcerTask = StartWatching();
        }

        private async Task StartWatching()
        {
            while (true)
            {
                if (_cancellationSource.IsCancellationRequested)
                {
                    return;
                }

                await Task.Delay(2500);
                var newTheme = WindowInteropUtils.GetUserThemePreference();
                if (newTheme != _currentTheme)
                {
                    _currentTheme = newTheme;
                    ThemeChanged(this);
                }
            }
        }

        public void Dispose()
        {
            _cancellationSource.Cancel();
            _wathcerTask.Wait();
        }
    }
}
