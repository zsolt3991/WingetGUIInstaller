﻿using CommunityToolkit.Mvvm.ComponentModel;
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
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Pages;

namespace WingetGUIInstaller.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {
        private bool _isConsoleEnabled;
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly GithubPackageUpdaterSerivce _updaterSerivce;
        private readonly Dictionary<string, Type> _pages = new()
        {
            { "list", typeof(ListPage) },
            { "recommend", typeof(RecommendationsPage) },
            { "search", typeof(SearchPage) },
            { "update", typeof(UpgradePage) },
            { "settings", typeof(SettingsPage) },
            { "console", typeof(ConsolePage) },
            { "about", typeof(AboutPage) },
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
                _dispatcherQueue.TryEnqueue(() => _update = m.Value);
                OnPropertyChanged(nameof(IsUpdateAvailable));
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

        public IReadOnlyDictionary<string, Type> Pages => _pages;

        public bool IsUpdateAvailable => !_update?.IsPackageUpToDate ?? false;

        public ICommand InstallUpdateCommand => new AsyncRelayCommand(InstallUpdateAsync);

        private async Task CheckForUpdatesAsync()
        {
            _update = await _updaterSerivce.CheckForUpdates(Package.Current);
            _dispatcherQueue.TryEnqueue(() => OnPropertyChanged(nameof(IsUpdateAvailable)));
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
