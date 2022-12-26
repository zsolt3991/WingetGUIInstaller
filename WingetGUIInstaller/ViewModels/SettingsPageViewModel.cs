using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using GithubPackageUpdater.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class SettingsPageViewModel : ObservableObject
    {
        private readonly NavigationItemKey[] _disallowedKeys = new NavigationItemKey[] {
            NavigationItemKey.Settings, NavigationItemKey.About, NavigationItemKey.Console, NavigationItemKey.Home };
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly GithubPackageUpdaterSerivce _updaterSerivce;
        private bool? _advancedFunctionalityEnabled;
        private bool? _notificationsEnabled;
        private bool? _automaticUpdatesEnabled;
        private NavigationItemKey? _selectedPage;
        private DisplayTheme? _selectedTheme;

        public SettingsPageViewModel(ApplicationDataStorageHelper configurationStore,
           GithubPackageUpdaterSerivce updaterSerivce)
        {
            _configurationStore = configurationStore;
            _updaterSerivce = updaterSerivce;
        }

        public bool AdvancedFunctionalityEnabled
        {
            get => _advancedFunctionalityEnabled ??= _configurationStore
                .Read(ConfigurationPropertyKeys.AdvancedFunctionalityEnabled, ConfigurationPropertyKeys.AdvancedFunctionalityEnabledDefaultValue);
            set
            {
                if (SetProperty(ref _advancedFunctionalityEnabled, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.AdvancedFunctionalityEnabled, value);
                    WeakReferenceMessenger.Default.Send(new ConsoleEnabledChangeMessage(_advancedFunctionalityEnabled.Value));
                }
            }
        }

        public bool NotificationsEnabled
        {
            get => _notificationsEnabled ??= _configurationStore
                .Read(ConfigurationPropertyKeys.NotificationsEnabled, ConfigurationPropertyKeys.NotificationsEnabledDefaultValue);
            set
            {
                if (SetProperty(ref _notificationsEnabled, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.NotificationsEnabled, value);
                }
            }
        }       

        public bool AutomaticUpdatesEnabled
        {
            get => _automaticUpdatesEnabled ??= _configurationStore
                .Read(ConfigurationPropertyKeys.AutomaticUpdates, ConfigurationPropertyKeys.AutomaticUpdatesDefaultValue);
            set
            {
                if (SetProperty(ref _automaticUpdatesEnabled, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.AutomaticUpdates, value);
                }
            }
        }

        public NavigationItemKey SelectedPage
        {
            get => _selectedPage ??= (NavigationItemKey)_configurationStore
                .Read(ConfigurationPropertyKeys.SelectedPage, ConfigurationPropertyKeys.SelectedPageDefaultValue);
            set
            {
                if (SetProperty(ref _selectedPage, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.SelectedPage, (int)value);
                }
            }
        }

        public DisplayTheme SelectedTheme
        {
            get => _selectedTheme ??= (DisplayTheme)_configurationStore
                .Read(ConfigurationPropertyKeys.SelectedTheme, ConfigurationPropertyKeys.SelectedThemeDefaultValue);
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.SelectedTheme, (int)value);
                    WeakReferenceMessenger.Default.Send(new ThemeChangedMessage((ElementTheme)value));
                }
            }
        }

        public IReadOnlyList<NavigationItemKey> AvailablePages => Enum.GetValues<NavigationItemKey>().Cast<NavigationItemKey>()
            .Where(key => !_disallowedKeys.Contains(key)).ToList();

        public IReadOnlyList<DisplayTheme> ApplicationThemes => Enum.GetValues<DisplayTheme>().Cast<DisplayTheme>().ToList();

        [RelayCommand]
        private async Task CheckForUpdatesAsync()
        {
            var checkResult = await _updaterSerivce.CheckForUpdates(Package.Current);
            if (!checkResult.IsPackageUpToDate)
            {
                WeakReferenceMessenger.Default.Send(new UpdateAvailableMessage(checkResult));
            }
        }
    }
}
