using CommunityToolkit.Common.Extensions;
using CommunityToolkit.Common.Helpers;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Utils;

namespace WingetGUIInstaller
{
    public sealed partial class MainWindow : Window
    {
        private readonly IMultiLevelNavigationService<NavigationItemKey> _navigationService;
        private readonly ISettingsStorageHelper<string> _applicationDataStorageHelper;
        private readonly ThemeListenerWithWindow _themeListener;

        public MainWindow()
        {
            InitializeComponent();

            _themeListener = new ThemeListenerWithWindow(this);
            _navigationService = Ioc.Default.GetRequiredService<IMultiLevelNavigationService<NavigationItemKey>>();
            _applicationDataStorageHelper = Ioc.Default.GetRequiredService<ISettingsStorageHelper<string>>();
            _navigationService.AddNavigationLevel(RootFrame);
            _navigationService.Navigate(NavigationItemKey.Home, null);
            _themeListener.ThemeChanged += ThemeListener_ThemeChanged;
#if UNPACKAGED
            AppWindow.Title = "Winget GUI Installer";
#else
            AppWindow.Title = Package.Current.DisplayName;
#endif
            AppWindow.SetIcon("icon.ico");

            if (WindowInteropUtils.IsWindowsBuildGreater(22000))
            {
                SystemBackdrop = new MicaBackdrop();
            }

            if (UserTheme == ElementTheme.Default)
            {
                var currentPreference = _themeListener.CurrentTheme;
                RootFrame.RequestedTheme = currentPreference.ToElementTheme();
                WindowInteropUtils.SetWin32ApplicationTheme(currentPreference, this);
            }
            else
            {
                RootFrame.RequestedTheme = UserTheme;
                WindowInteropUtils.SetWin32ApplicationTheme(UserTheme.ToApplicationTheme(), this);
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
            .GetValueOrDefault(ConfigurationPropertyKeys.SelectedTheme, ConfigurationPropertyKeys.SelectedThemeDefaultValue);

        private void OnThemeChangeRequestedByUser(object recipient, ThemeChangedMessage message)
        {
            var newTheme = message.Value;
            if (newTheme != RootFrame.ActualTheme)
            {
                if (newTheme == ElementTheme.Default)
                {
                    var currentPreference = WindowInteropUtils.GetUserThemePreference();
                    RootFrame.RequestedTheme = currentPreference.ToElementTheme();
                    WindowInteropUtils.SetWin32ApplicationTheme(currentPreference, this);
                }
                else
                {
                    RootFrame.RequestedTheme = newTheme;
                    WindowInteropUtils.SetWin32ApplicationTheme(newTheme.ToApplicationTheme(), this);
                }
            }
        }

        private void ThemeListener_ThemeChanged(ThemeListenerWithWindow sender)
        {
            ElementTheme newTheme = sender.CurrentTheme.ToElementTheme();
            if (UserTheme == ElementTheme.Default && newTheme != RootFrame.ActualTheme)
            {
                RootFrame.RequestedTheme = newTheme;
                WindowInteropUtils.SetWin32ApplicationTheme(newTheme.ToApplicationTheme(), this);
            }
        }
    }
}
