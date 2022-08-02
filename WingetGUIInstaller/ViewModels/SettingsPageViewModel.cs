using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using GithubPackageUpdater.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;

namespace WingetGUIInstaller.ViewModels
{
    public class SettingsPageViewModel : ObservableObject
    {
        private readonly NavigationItemKey[] _disallowedKeys = new NavigationItemKey[] {
            NavigationItemKey.Settings, NavigationItemKey.About, NavigationItemKey.Console, NavigationItemKey.Home };
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly GithubPackageUpdaterSerivce _updaterSerivce;
        private bool? _consoleTabEnabled;
        private bool? _notificationsEnabled;
        private bool? _packageSourceFilteringEnabled;
        private bool? _ignoreEmptyPackageSourceEnabled;
        private bool? _automaticUpdatesEnabled;
        private NavigationItemKey? _selectedPage;


        public SettingsPageViewModel(ApplicationDataStorageHelper configurationStore,
           GithubPackageUpdaterSerivce updaterSerivce)
        {
            _configurationStore = configurationStore;
            _updaterSerivce = updaterSerivce;
        }


        public bool ConsoleTabEnabled
        {
            get
            {
                if (_consoleTabEnabled == null)
                {
                    _consoleTabEnabled = _configurationStore
                        .Read(ConfigurationPropertyKeys.ConsoleEnabled, ConfigurationPropertyKeys.ConsoleEnabledDefaultValue);
                }
                return _consoleTabEnabled.Value;
            }
            set
            {
                if (SetProperty(ref _consoleTabEnabled, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.ConsoleEnabled, value);
                    WeakReferenceMessenger.Default.Send(new ConsoleEnabledChangeMessage(_consoleTabEnabled.Value));
                }
            }
        }

        public bool NotificationsEnabled
        {
            get
            {
                if (_notificationsEnabled == null)
                {
                    _notificationsEnabled = _configurationStore
                        .Read(ConfigurationPropertyKeys.NotificationsEnabled, ConfigurationPropertyKeys.NotificationsEnabledDefaultValue);
                }
                return _notificationsEnabled.Value;
            }
            set
            {
                if (SetProperty(ref _notificationsEnabled, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.NotificationsEnabled, value);
                }
            }
        }

        public bool PackageSourceFilteringEnabled
        {
            get
            {
                if (_packageSourceFilteringEnabled == null)
                {
                    _packageSourceFilteringEnabled = _configurationStore
                        .Read(ConfigurationPropertyKeys.PackageSourceFilteringEnabled, ConfigurationPropertyKeys.PackageSourceFilteringEnabledDefaultValue);
                }
                return _packageSourceFilteringEnabled.Value;
            }
            set
            {
                if (SetProperty(ref _packageSourceFilteringEnabled, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.PackageSourceFilteringEnabled, value);
                }
            }
        }

        public bool IgnoreEmptyPackageSourceEnabled
        {
            get
            {
                if (_ignoreEmptyPackageSourceEnabled == null)
                {
                    _ignoreEmptyPackageSourceEnabled = _configurationStore
                        .Read(ConfigurationPropertyKeys.IgnoreEmptyPackageSources, ConfigurationPropertyKeys.IgnoreEmptyPackageSourcesDefaultValue);
                }
                return _ignoreEmptyPackageSourceEnabled.Value;
            }
            set
            {
                if (SetProperty(ref _ignoreEmptyPackageSourceEnabled, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.IgnoreEmptyPackageSources, value);
                }
            }
        }

        public bool AutomaticUpdatesEnabled
        {
            get
            {
                if (_automaticUpdatesEnabled == null)
                {
                    _automaticUpdatesEnabled = _configurationStore
                        .Read(ConfigurationPropertyKeys.AutomaticUpdates, ConfigurationPropertyKeys.AutomaticUpdatesDefaultValue);
                }
                return _automaticUpdatesEnabled.Value;
            }
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
            get
            {
                if (_selectedPage == null)
                {
                    _selectedPage = (NavigationItemKey)_configurationStore
                        .Read(ConfigurationPropertyKeys.SelectedPage, ConfigurationPropertyKeys.SelectedPageDefaultValue);
                }
                return _selectedPage.Value;
            }
            set
            {
                if (SetProperty(ref _selectedPage, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.SelectedPage, (int)value);
                }
            }
        }

        public IReadOnlyList<NavigationItemKey> AvailablePages => Enum.GetValues<NavigationItemKey>().Cast<NavigationItemKey>()
            .Where(key => !_disallowedKeys.Contains(key)).ToList();

        public ICommand CheckForUpdatesCommand => new AsyncRelayCommand(CheckForUpdatesAsync);

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
