using CommunityToolkit.Common.Extensions;
using CommunityToolkit.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GithubPackageUpdater.Models;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Services;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class HomePageViewModel : ObservableObject
    {
        private readonly ISettingsStorageHelper<string> _configurationStore;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ApplicationUpdateManager _updaterSerivce;
        private readonly ILogger<HomePageViewModel> _logger;

        [ObservableProperty]
        private bool _isAdvancedModeEnabled;

        [ObservableProperty]
        private bool _isNavigationAllowed;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUpdateAvailable))]
        [NotifyPropertyChangedFor(nameof(UpdateChangeLog))]
        [NotifyPropertyChangedFor(nameof(UpdateVersion))]
        private PackageUpdateResponse _update;

        public HomePageViewModel(ISettingsStorageHelper<string> configurationStore, DispatcherQueue dispatcherQueue,
            ApplicationUpdateManager updaterSerivce, ILogger<HomePageViewModel> logger)
        {
            _configurationStore = configurationStore;
            _dispatcherQueue = dispatcherQueue;
            _updaterSerivce = updaterSerivce;
            _logger = logger;
            _isNavigationAllowed = true;

            WeakReferenceMessenger.Default.Register<ConsoleEnabledChangeMessage>(this, (r, m) =>
            {
                _dispatcherQueue.TryEnqueue(() => IsAdvancedModeEnabled = m.Value);
            });

            WeakReferenceMessenger.Default.Register<UpdateAvailableMessage>(this, (r, m) =>
            {
                _dispatcherQueue.TryEnqueue(() => { Update = m.Value; });
            });

            WeakReferenceMessenger.Default.Register<TopLevelNavigationAllowedMessage>(this, (r, m) =>
            {
                _dispatcherQueue.TryEnqueue(() => { IsNavigationAllowed = m.Value; });
            });

            IsAdvancedModeEnabled = _configurationStore
                .GetValueOrDefault(ConfigurationPropertyKeys.AdvancedFunctionalityEnabled, ConfigurationPropertyKeys.AdvancedFunctionalityEnabledDefaultValue);

            if (CheckForUpdate)
            {
                _ = CheckForUpdatesAsync();
            }
        }

        public bool IsUpdateAvailable => !Update?.IsPackageUpToDate ?? false;

        public Version UpdateVersion => Update?.AvailableUpdateVersion ?? default;

        public string UpdateChangeLog => Update?.ChangeLog ?? string.Empty;

        public bool CheckForUpdate => _configurationStore
            .GetValueOrDefault(ConfigurationPropertyKeys.AutomaticUpdates, ConfigurationPropertyKeys.AutomaticUpdatesDefaultValue);

        [RelayCommand]
        private async Task InstallUpdateAsync()
        {
            if (Update != default)
            {
                await _updaterSerivce.ApplyApplicationUpdate(Update);
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            var checkResult = await _updaterSerivce.CheckForApplicationUpdate();
            if (checkResult != default && !checkResult.IsPackageUpToDate)
            {
                _dispatcherQueue.TryEnqueue(() => { Update = checkResult; });
            }
        }
    }
}
