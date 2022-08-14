using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using CommunityToolkit.WinUI.UI.Helpers;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using Windows.ApplicationModel;
using Windows.UI;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Services;
using WinRT.Interop;

namespace WingetGUIInstaller
{
    public sealed partial class MainWindow : Window
    {
        private readonly IMultiLevelNavigationService<NavigationItemKey> _navigationService;
        private readonly ApplicationDataStorageHelper _applicationDataStorageHelper;
        private readonly ThemeListener _themeListener;

        public MainWindow()
        {
            InitializeComponent();
            SetTitleBar();

            _themeListener = new ThemeListener(Ioc.Default.GetRequiredService<DispatcherQueue>());
            _navigationService = Ioc.Default.GetRequiredService<IMultiLevelNavigationService<NavigationItemKey>>();
            _applicationDataStorageHelper = Ioc.Default.GetRequiredService<ApplicationDataStorageHelper>();
            _navigationService.AddNavigationLevel(RootFrame);
            _navigationService.Navigate(NavigationItemKey.Home, null);
            _themeListener.ThemeChanged += ThemeListener_ThemeChanged;

            RootFrame.RequestedTheme = UserTheme == ElementTheme.Default ? ToElementTheme(_themeListener.CurrentTheme) : UserTheme;
            WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(this, OnThemeChangeRequestedByUser);

            Closed += (sender, args) =>
            {
                RootFrame = null;
                _navigationService.ClearNavigationStack();
            };
        }

        private ElementTheme UserTheme => (ElementTheme)_applicationDataStorageHelper
            .Read(ConfigurationPropertyKeys.SelectedTheme, ConfigurationPropertyKeys.SelectedPageDefaultValue);

        private void OnThemeChangeRequestedByUser(object recipient, ThemeChangedMessage message)
        {
            var newTheme = message.Value;
            if (newTheme != RootFrame.ActualTheme)
            {
                RootFrame.RequestedTheme = newTheme;
            }
        }

        private void ThemeListener_ThemeChanged(ThemeListener sender)
        {
            ElementTheme newTheme = ToElementTheme(sender.CurrentTheme);
            if (UserTheme == ElementTheme.Default && newTheme != RootFrame.ActualTheme)
            {
                RootFrame.RequestedTheme = newTheme;
            }
        }

        private static ElementTheme ToElementTheme(ApplicationTheme applictionTheme)
        {
            return applictionTheme switch
            {
                ApplicationTheme.Dark => ElementTheme.Dark,
                ApplicationTheme.Light => ElementTheme.Light,
                _ => ElementTheme.Default
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
