﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using GithubPackageUpdater.Models;
using GithubPackageUpdater.Services;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Messages;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class HomePageViewModel : ObservableObject
    {
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly GithubPackageUpdaterSerivce _updaterSerivce;
        private readonly ILogger<HomePageViewModel> _logger;
        [ObservableProperty]
        private bool _isAdvancedModeEnabled;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUpdateAvailable))]
        [NotifyPropertyChangedFor(nameof(UpdateChangeLog))]
        [NotifyPropertyChangedFor(nameof(UpdateVersion))]
        private PackageUpdateResponse _update;

        public HomePageViewModel(ApplicationDataStorageHelper configurationStore, DispatcherQueue dispatcherQueue,
            GithubPackageUpdaterSerivce updaterSerivce, ILogger<HomePageViewModel> logger)
        {
            _configurationStore = configurationStore;
            _dispatcherQueue = dispatcherQueue;
            _updaterSerivce = updaterSerivce;
            _logger = logger;

            WeakReferenceMessenger.Default.Register<ConsoleEnabledChangeMessage>(this, (r, m) =>
            {
                _dispatcherQueue.TryEnqueue(() => IsAdvancedModeEnabled = m.Value);
            });

            WeakReferenceMessenger.Default.Register<UpdateAvailableMessage>(this, (r, m) =>
            {
                _dispatcherQueue.TryEnqueue(() => { Update = m.Value; });
            });

            IsAdvancedModeEnabled = _configurationStore
                .Read(ConfigurationPropertyKeys.AdvancedFunctionalityEnabled, ConfigurationPropertyKeys.AdvancedFunctionalityEnabledDefaultValue);

            if (CheckForUpdate)
            {
                _ = CheckForUpdatesAsync();
            }
        }

        public bool IsUpdateAvailable => !Update?.IsPackageUpToDate ?? false;

        public Version UpdateVersion => Update?.AvailableUpdateVersion ?? default;

        public string UpdateChangeLog => Update?.ChangeLog ?? string.Empty;

        public bool CheckForUpdate => _configurationStore
            .Read(ConfigurationPropertyKeys.AutomaticUpdates, ConfigurationPropertyKeys.AutomaticUpdatesDefaultValue);

        [RelayCommand]
        private async Task InstallUpdateAsync()
        {
            if (Update != default)
            {
                await _updaterSerivce.TriggerUpdate(Update.PackageUri);
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var checkResult = await _updaterSerivce.CheckForUpdates(Package.Current);
                if (checkResult != default && !checkResult.IsPackageUpToDate)
                {
                    _dispatcherQueue.TryEnqueue(() => { Update = checkResult; });
                }
            }
            catch (Exception updateException)
            {
                _logger.LogError(updateException, "Checking for updates failed with error:");
            }
        }
    }
}