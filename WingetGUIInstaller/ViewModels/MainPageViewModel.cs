using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using GithubPackageUpdater.Models;
using GithubPackageUpdater.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Pages;

namespace WingetGUIInstaller.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {
        private bool _isConsoleEnabled;
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly GithubPackageUpdaterSerivce _updaterSerivce;
        private readonly Dictionary<NavigationItemKey, Type> _pages = new()
        {
            { NavigationItemKey.InstalledPackages, typeof(ListPage) },
            { NavigationItemKey.Recommendations, typeof(RecommendationsPage) },
            { NavigationItemKey.Search, typeof(SearchPage) },
            { NavigationItemKey.Upgrades, typeof(UpgradePage) },
            { NavigationItemKey.Settings, typeof(SettingsPage) },
            { NavigationItemKey.Console, typeof(ConsolePage) },
            { NavigationItemKey.About, typeof(AboutPage) },
        };

        private PackageUpdateResponse _update;

        public MainPageViewModel(ApplicationDataStorageHelper configurationStore, DispatcherQueue dispatcherQueue,
            GithubPackageUpdaterSerivce updaterSerivce)
        {
            _configurationStore = configurationStore;
            _dispatcherQueue = dispatcherQueue;
            _updaterSerivce = updaterSerivce;

            WeakReferenceMessenger.Default.Register<ConsoleEnabledChangeMessage>(this, (r, m) =>
            {
                _dispatcherQueue.TryEnqueue(() => IsConsoleEnabled = m.Value);
            });

            WeakReferenceMessenger.Default.Register<UpdateAvailableMessage>(this, (r, m) =>
            {
                _update = m.Value;
                _dispatcherQueue.TryEnqueue(() =>
                {
                    OnPropertyChanged(nameof(IsUpdateAvailable));
                    OnPropertyChanged(nameof(UpdateChangeLog));
                    OnPropertyChanged(nameof(UpdateVersion));
                });
            });

            IsConsoleEnabled = _configurationStore
                .Read(ConfigurationPropertyKeys.ConsoleEnabled, ConfigurationPropertyKeys.ConsoleEnabledDefaultValue);

            var checkForUpdate = _configurationStore
                .Read(ConfigurationPropertyKeys.AutomaticUpdates, ConfigurationPropertyKeys.AutomaticUpdatesDefaultValue);

            if (checkForUpdate)
            {
                _ = CheckForUpdatesAsync();
            }
        }

        public bool IsConsoleEnabled
        {
            get => _isConsoleEnabled;
            set => SetProperty(ref _isConsoleEnabled, value);
        }

        public IReadOnlyDictionary<NavigationItemKey, Type> Pages => _pages;

        public bool IsUpdateAvailable => !_update?.IsPackageUpToDate ?? false;

        public Version UpdateVersion => _update?.AvailableUpdateVersion ?? default;

        public string UpdateChangeLog => _update?.ChangeLog ?? string.Empty;

        public ICommand InstallUpdateCommand => new AsyncRelayCommand(InstallUpdateAsync);

        private async Task CheckForUpdatesAsync()
        {
            _update = await _updaterSerivce.CheckForUpdates(Package.Current);
            _dispatcherQueue.TryEnqueue(() =>
            {
                OnPropertyChanged(nameof(IsUpdateAvailable));
                OnPropertyChanged(nameof(UpdateChangeLog));
                OnPropertyChanged(nameof(UpdateVersion));
            });
        }

        private async Task InstallUpdateAsync()
        {
            if (_update != default)
            {
                await _updaterSerivce.TriggerUpdate(_update.PackageUri);
            }
        }
    }
}
