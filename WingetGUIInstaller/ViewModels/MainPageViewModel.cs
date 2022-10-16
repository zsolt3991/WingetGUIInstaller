using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using GithubPackageUpdater.Models;
using GithubPackageUpdater.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Messages;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class MainPageViewModel : ObservableObject
    {
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly GithubPackageUpdaterSerivce _updaterSerivce;

        [ObservableProperty]
        private bool _isConsoleEnabled;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUpdateAvailable))]
        [NotifyPropertyChangedFor(nameof(UpdateChangeLog))]
        [NotifyPropertyChangedFor(nameof(UpdateVersion))]
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
                _dispatcherQueue.TryEnqueue(() => { Update = m.Value; });
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

        public bool IsUpdateAvailable => !Update?.IsPackageUpToDate ?? false;

        public Version UpdateVersion => Update?.AvailableUpdateVersion ?? default;

        public string UpdateChangeLog => Update?.ChangeLog ?? string.Empty;

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
            var checkResult = await _updaterSerivce.CheckForUpdates(Package.Current);
            if (checkResult != default)
            {
                _dispatcherQueue.TryEnqueue(() => { Update = checkResult; });
            }
        }
    }
}
