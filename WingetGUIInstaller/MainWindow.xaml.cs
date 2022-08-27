using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Win32;
using System;
using Windows.ApplicationModel;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;
using WinRT.Interop;

namespace WingetGUIInstaller
{
    public sealed partial class MainWindow : Window
    {
        private readonly IMultiLevelNavigationService<NavigationItemKey> _navigationService;
        private readonly ApplicationDataStorageHelper _applicationDataStorageHelper;
        private readonly ThemeListenerWithWindow _themeListener;

        public MainWindow()
        {
            InitializeComponent();
            SetTitleBar();
            _themeListener = new ThemeListenerWithWindow(this);
            _navigationService = Ioc.Default.GetRequiredService<IMultiLevelNavigationService<NavigationItemKey>>();
            _applicationDataStorageHelper = Ioc.Default.GetRequiredService<ApplicationDataStorageHelper>();
            _navigationService.AddNavigationLevel(RootFrame);
            _navigationService.Navigate(NavigationItemKey.Home, null);
            _themeListener.ThemeChanged += ThemeListener_ThemeChanged;

            if (UserTheme == ElementTheme.Default)
            {
                var currentPreference = _themeListener.CurrentTheme;
                RootFrame.RequestedTheme = ToElementTheme(currentPreference);
                WindowInteropUtils.SetWin32ApplicationTheme(currentPreference, this);
            }
            else
            {
                RootFrame.RequestedTheme = UserTheme;
                WindowInteropUtils.SetWin32ApplicationTheme(ToApplicationTheme(UserTheme), this);
            }

            WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(this, OnThemeChangeRequestedByUser);

            Closed += (sender, args) =>
            {
                RootFrame = null;
                _themeListener.Dispose();
                _navigationService.ClearNavigationStack();
            };
        }

        private ElementTheme UserTheme => (ElementTheme)_applicationDataStorageHelper
            .Read(ConfigurationPropertyKeys.SelectedTheme, ConfigurationPropertyKeys.SelectedThemeDefaultValue);

        private void OnThemeChangeRequestedByUser(object recipient, ThemeChangedMessage message)
        {
            var newTheme = message.Value;
            if (newTheme != RootFrame.ActualTheme)
            {
                if (newTheme == ElementTheme.Default)
                {
                    var currentPreference = WindowInteropUtils.GetUserThemePreference();
                    RootFrame.RequestedTheme = ToElementTheme(currentPreference);
                    WindowInteropUtils.SetWin32ApplicationTheme(currentPreference, this);
                }
                else
                {
                    RootFrame.RequestedTheme = newTheme;
                    WindowInteropUtils.SetWin32ApplicationTheme(ToApplicationTheme(newTheme), this);
                }
            }
        }

        private void ThemeListener_ThemeChanged(ThemeListenerWithWindow sender)
        {
            ElementTheme newTheme = ToElementTheme(sender.CurrentTheme);
            if (UserTheme == ElementTheme.Default && newTheme != RootFrame.ActualTheme)
            {
                RootFrame.RequestedTheme = newTheme;
                WindowInteropUtils.SetWin32ApplicationTheme(ToApplicationTheme(newTheme), this);
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

        private ApplicationTheme ToApplicationTheme(ElementTheme elementTheme)
        {
            return elementTheme switch
            {
                ElementTheme.Dark => ApplicationTheme.Dark,
                ElementTheme.Light => ApplicationTheme.Light,
                _ => ApplicationTheme.Light
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
            m_AppWindow.SetIcon("icon.ico");
        }
    }
}
