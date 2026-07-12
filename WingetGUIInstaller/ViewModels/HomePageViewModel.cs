using CommunityToolkit.Common.Extensions;
using CommunityToolkit.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Messages;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class HomePageViewModel : ObservableObject
    {
        private readonly ISettingsStorageHelper<string> _configurationStore;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly IUpdateService _updateService;
        private readonly ILogger<HomePageViewModel> _logger;

        [ObservableProperty]
        private bool _isAdvancedModeEnabled;

        [ObservableProperty]
        private bool _isNavigationAllowed;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUpdateAvailable))]
        [NotifyPropertyChangedFor(nameof(UpdateChangeLog))]
        [NotifyPropertyChangedFor(nameof(UpdateVersion))]
        private IUpdateResponse _update;

        public HomePageViewModel(ISettingsStorageHelper<string> configurationStore, DispatcherQueue dispatcherQueue,
            IUpdateService updateService, ILogger<HomePageViewModel> logger)
        {
            _configurationStore = configurationStore;
            _dispatcherQueue = dispatcherQueue;
            _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));
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

        public bool IsUpdateAvailable => Update?.IsUpdateAvailable ?? false;

        public Version UpdateVersion => Update?.UpdateVersion ?? default;

        public string UpdateChangeLog => Update?.ChangeLog ?? string.Empty;

        public bool CheckForUpdate => _configurationStore
            .GetValueOrDefault(ConfigurationPropertyKeys.AutomaticUpdates, ConfigurationPropertyKeys.AutomaticUpdatesDefaultValue);

        [RelayCommand]
        private async Task InstallUpdateAsync()
        {
            if (Update?.IsUpdateAvailable == true)
            {
                _logger.LogInformation("Update Now clicked for version {Version}", Update.UpdateVersion);
                await _updateService.InstallUpdateAsync(Update.UpdateUri);
                _logger.LogInformation("Update installation request completed");
            }
            else
            {
                _logger.LogWarning("Update Now clicked but no update is available");
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                _logger.LogInformation("HomePage checking for updates");

                var checkResult = await _updateService.CheckForUpdatesAsync();
                if (checkResult?.IsUpdateAvailable ?? false)
                {
                    _logger.LogInformation("Update available in HomePage: {Version}", checkResult.UpdateVersion);
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
