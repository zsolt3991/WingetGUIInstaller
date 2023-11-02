using CommunityToolkit.Common.Extensions;
using CommunityToolkit.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GithubPackageUpdater.Services;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class SettingsPageViewModel : ObservableObject
    {
        private readonly NavigationItemKey[] _disallowedKeys = new NavigationItemKey[] {
            NavigationItemKey.Settings, NavigationItemKey.About, NavigationItemKey.Console, NavigationItemKey.Home };
        private readonly ISettingsStorageHelper<string> _configurationStore;
        private readonly GithubPackageUpdaterSerivce _updaterSerivce;
        private readonly ILogger<SettingsPageViewModel> _logger;
        private readonly IReadOnlyList<ApplicationDisplayLanguage> _applicationLanguages;
        private readonly IReadOnlyList<DisplayTheme> _availableThemes;
        private readonly IReadOnlyList<LogLevel> _availableLogLevels;
        private readonly IReadOnlyList<NavigationItemKey> _availablePages;
        private bool? _advancedFunctionalityEnabled;
        private bool? _notificationsEnabled;
        private bool? _automaticUpdatesEnabled;
        private LogLevel? _selectedLogLevel;
        private NavigationItemKey? _selectedPage;
        private DisplayTheme? _selectedTheme;
        private ApplicationDisplayLanguage _selectedDisplayLanguage;

        public SettingsPageViewModel(ISettingsStorageHelper<string> configurationStore,
           GithubPackageUpdaterSerivce updaterSerivce, ILogger<SettingsPageViewModel> logger)
        {
            _configurationStore = configurationStore;
            _updaterSerivce = updaterSerivce;
            _logger = logger;
            _applicationLanguages = new List<ApplicationDisplayLanguage>
            {
                new ApplicationDisplayLanguage{ FriendlyName = "System Default", CultureCode = "default"},
                new ApplicationDisplayLanguage{ FriendlyName = "English", CultureCode = "en-US"},
            };
            _availableThemes = Enum.GetValues<DisplayTheme>();
            _availableLogLevels = Enum.GetValues<LogLevel>();
            _availablePages = Enum.GetValues<NavigationItemKey>().Where(key => !_disallowedKeys.Contains(key)).ToList();
        }

        public bool AdvancedFunctionalityEnabled
        {
            get => _advancedFunctionalityEnabled ??= _configurationStore
                .GetValueOrDefault(ConfigurationPropertyKeys.AdvancedFunctionalityEnabled, ConfigurationPropertyKeys.AdvancedFunctionalityEnabledDefaultValue);
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
                .GetValueOrDefault(ConfigurationPropertyKeys.NotificationsEnabled, ConfigurationPropertyKeys.NotificationsEnabledDefaultValue);
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
                .GetValueOrDefault(ConfigurationPropertyKeys.AutomaticUpdates, ConfigurationPropertyKeys.AutomaticUpdatesDefaultValue);
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
                .GetValueOrDefault(ConfigurationPropertyKeys.SelectedPage, ConfigurationPropertyKeys.SelectedPageDefaultValue);
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
                .GetValueOrDefault(ConfigurationPropertyKeys.SelectedTheme, ConfigurationPropertyKeys.SelectedThemeDefaultValue);
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.SelectedTheme, (int)value);
                    WeakReferenceMessenger.Default.Send(new ThemeChangedMessage((ElementTheme)value));
                }
            }
        }

        public LogLevel SelectedLogLevel
        {
            get
            {
                _selectedLogLevel ??= (LogLevel)_configurationStore
                    .GetValueOrDefault(ConfigurationPropertyKeys.LogLevel, ConfigurationPropertyKeys.DefaultLogLevel);
                return _selectedLogLevel.Value;
            }
            set
            {
                if (SetProperty(ref _selectedLogLevel, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.LogLevel, (int)value);
                }
            }
        }

        public ApplicationDisplayLanguage SelectedLanguage
        {
            get
            {
                if (_selectedDisplayLanguage == null)
                {
                    var selectedDisplayLanguage = (string)_configurationStore
                        .GetValueOrDefault(ConfigurationPropertyKeys.ApplicationLanguageOverride, ConfigurationPropertyKeys.ApplicationLanguageOverrideDefaultValue);
                    _selectedDisplayLanguage = _applicationLanguages.FirstOrDefault(lang => lang.CultureCode == selectedDisplayLanguage) ?? _applicationLanguages[0];
                }
                return _selectedDisplayLanguage;
            }
            set
            {
                if (SetProperty(ref _selectedDisplayLanguage, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.ApplicationLanguageOverride, _selectedDisplayLanguage.CultureCode);
                }
            }
        }

        public IReadOnlyList<LogLevel> LogLevels => _availableLogLevels;

        public IReadOnlyList<NavigationItemKey> AvailablePages => _availablePages;

        public IReadOnlyList<DisplayTheme> ApplicationThemes => _availableThemes;

        public IReadOnlyList<ApplicationDisplayLanguage> ApplicationLanguages => _applicationLanguages;

        [RelayCommand]
        private async Task OpenLogsFolder()
        {
            await LogStorageHelper.OpenLogFileDirectory();
        }

        [RelayCommand]
        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var checkResult = await _updaterSerivce.CheckForUpdates(Package.Current);
                if (checkResult != default)
                {
                    WeakReferenceMessenger.Default.Send(new UpdateAvailableMessage(checkResult));
                }
            }
            catch (Exception updateException)
            {
                _logger.LogError(updateException, "Checking for updates failed with error:");
            }
        }
    }
}
