using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using Windows.ApplicationModel;
using Windows.UI;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Services;
using WinRT.Interop;

namespace WingetGUIInstaller
{
    public sealed partial class MainWindow : Window
    {
        private readonly IMultiLevelNavigationService<NavigationItemKey> _navigationService;

        public MainWindow()
        {
            InitializeComponent();
            SetTitleBar();

            _navigationService = Ioc.Default.GetRequiredService<IMultiLevelNavigationService<NavigationItemKey>>();
            _navigationService.AddNavigationLevel(RootFrame);
            _navigationService.Navigate(NavigationItemKey.Home, null);

            Closed += (sender, args) =>
            {
                RootFrame = null;
                _navigationService.ClearNavigationStack();
            };
        }

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        private void SetTitleBar()
        {
            var m_AppWindow = GetAppWindowForCurrentWindow();
            m_AppWindow.Title = Package.Current.DisplayName;

            // Check to see if customization is supported.
            // Currently only supported on Windows 11.
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = m_AppWindow.TitleBar;

                // Set active window colors
                titleBar.ForegroundColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
                titleBar.BackgroundColor = Color.FromArgb(0xFF, 0x1D, 0x1D, 0x1D);

                // Set inactive window colors
                titleBar.InactiveForegroundColor = Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF);
                titleBar.InactiveBackgroundColor = Color.FromArgb(0x66, 0x1D, 0x1D, 0x1D);
                titleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;

                titleBar.ButtonBackgroundColor = Color.FromArgb(0xFF, 0x1D, 0x1D, 0x1D);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0xFF, 0x3F, 0x3F, 0x3F);
                titleBar.ButtonForegroundColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
                titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(0x66, 0x1D, 0x1D, 0x1D);
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(0x66, 0xAE, 0xAE, 0xAE);

            }
        }
    }
}
