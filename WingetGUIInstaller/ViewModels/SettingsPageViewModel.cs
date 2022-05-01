﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using GithubPackageUpdater.Services;
using Microsoft.UI.Dispatching;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Services;
using WingetHelper.Commands;

namespace WingetGUIInstaller.ViewModels
{
    public class SettingsPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ConsoleOutputCache _cache;
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly GithubPackageUpdaterSerivce _updaterSerivce;
        private readonly List<string> _disabledPackageSources;
        private ObservableCollection<WingetPackageSourceViewModel> _packageSources;
        private WingetPackageSourceViewModel _selectedSource;
        private string _newPackageSourceName;
        private string _newPackageSourceUrl;
        private bool? _consoleTabEnabled;
        private bool? _notificationsEnabled;
        private bool? _packageSourceFilteringEnabled;
        private bool? _ignoreEmptyPackageSourceEnabled;
        private bool? _automaticUpdatesEnabled;

        public SettingsPageViewModel(DispatcherQueue dispatcherQueue, ApplicationDataStorageHelper configurationStore,
           GithubPackageUpdaterSerivce updaterSerivce, ConsoleOutputCache cache)
        {
            _dispatcherQueue = dispatcherQueue;
            _configurationStore = configurationStore;
            _updaterSerivce = updaterSerivce;
            _cache = cache;
            _disabledPackageSources = LoadDisabledPackageSources();
            PackageSources = new ObservableCollection<WingetPackageSourceViewModel>();
            PackageSources.CollectionChanged += PackageSources_CollectionChanged;
            _ = LoadPackageSourcesAsync();
        }

        public ObservableCollection<WingetPackageSourceViewModel> PackageSources
        {
            get => _packageSources;
            set => SetProperty(ref _packageSources, value);
        }

        public WingetPackageSourceViewModel SelectedSource
        {
            get => _selectedSource;
            set => SetProperty(ref _selectedSource, value);
        }

        public string NewPackageSourceName
        {
            get => _newPackageSourceName;
            set => SetProperty(ref _newPackageSourceName, value);
        }

        public string NewPackageSourceUrl
        {
            get => _newPackageSourceUrl;
            set => SetProperty(ref _newPackageSourceUrl, value);
        }

        public bool ConsoleTabEnabled
        {
            get
            {
                if (_consoleTabEnabled == null)
                {
                    _consoleTabEnabled = _configurationStore
                        .Read(ConfigurationPropertyKeys.ConsoleEnabled, ConfigurationPropertyKeys.ConsoleEnabledDefaultValue);
                    WeakReferenceMessenger.Default.Send(new ConsoleEnabledChangeMessage(_consoleTabEnabled.Value));
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

        public int SelectedCount => PackageSources.Any(p => p.IsSelected) ?
            PackageSources.Count(p => p.IsSelected) : SelectedSource != default ? 1 : 0;

        public ICommand AddPackageSourceCommand => new AsyncRelayCommand(()
            => AddPackageSourceAsync(NewPackageSourceName, NewPackageSourceUrl));

        public ICommand RemoveSelectedSourcesCommand => new AsyncRelayCommand(()
            => RemovePackageSourcesAsync(PackageSources.Where(p => p.IsSelected).Select(p => p.Name)));

        public ICommand CheckForUpdatesCommand => new AsyncRelayCommand(CheckForUpdatesAsync);

        private async Task CheckForUpdatesAsync()
        {
            var checkResult = await _updaterSerivce.CheckForUpdates(Package.Current);
            if (!checkResult.IsPackageUpToDate)
            {
                WeakReferenceMessenger.Default.Send(new UpdateAvailableMessage(checkResult));
            }
        }

        private async Task LoadPackageSourcesAsync()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                PackageSources.Clear();
            });

            var wingetSources = await PackageSourceCommands.GetPackageSources()
                .ConfigureOutputListener(_cache.IngestMessage)
                .ExecuteAsync();
            foreach (var entry in wingetSources)
            {
                _dispatcherQueue.TryEnqueue(() => PackageSources.Add(new WingetPackageSourceViewModel
                {
                    IsSelected = false,
                    Name = entry.Name,
                    Url = entry.Argument,
                    IsEnabled = !_disabledPackageSources.Any(p => p == entry.Name)
                }));
            }
        }

        private async Task AddPackageSourceAsync(string name, string argument)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(argument))
            {
                return;
            }

            await PackageSourceCommands.AddPackageSource(name, argument)
                .ConfigureOutputListener(_cache.IngestMessage)
                .ExecuteAsync();
            await LoadPackageSourcesAsync();
        }

        private async Task RemovePackageSourcesAsync(IEnumerable<string> sourceNames)
        {
            if (sourceNames.Any())
            {
                foreach (var sourceName in sourceNames)
                {
                    await PackageSourceCommands.RemovePackageSource(sourceName)
                        .ConfigureOutputListener(_cache.IngestMessage)
                        .ExecuteAsync();
                }
                await LoadPackageSourcesAsync();
            }
        }

        private void PackageSources_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != default)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is WingetPackageSourceViewModel packageSourceViewModel)
                    {
                        packageSourceViewModel.PropertyChanged += OnPackagePropertyChanged;
                    }
                }
            }

            if ((e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace) && e.OldItems != default)
            {
                foreach (var item in e.OldItems)
                {

                    if (item is WingetPackageSourceViewModel packageSourceViewModel)
                    {
                        packageSourceViewModel.PropertyChanged -= OnPackagePropertyChanged;
                    }
                }
            }

            OnPropertyChanged(nameof(SelectedCount));
        }

        private void OnPackagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WingetPackageSourceViewModel.IsSelected):
                    OnPropertyChanged(nameof(SelectedCount));
                    break;
                case nameof(WingetPackageSourceViewModel.IsEnabled):
                    UpdateEnabledPackageList();
                    break;
                default:
                    break;
            }
        }

        private List<string> LoadDisabledPackageSources()
        {
            var serializedValue = _configurationStore
                .Read(ConfigurationPropertyKeys.DisabledPackageSources, ConfigurationPropertyKeys.DisabledPackageSourcesDefaultValue);
            return serializedValue?.Split(';', System.StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
        }

        private void SaveDisabledPackageSources(List<string> packageSources)
        {
            var serializedValue = string.Join(';', packageSources);
            _configurationStore.Save(ConfigurationPropertyKeys.DisabledPackageSources, serializedValue);
        }

        private void UpdateEnabledPackageList()
        {
            foreach (var packageSource in PackageSources.Where(p => !p.IsEnabled))
            {
                if (_disabledPackageSources.Contains(packageSource.Name))
                    continue;
                _disabledPackageSources.Add(packageSource.Name);
            }
            SaveDisabledPackageSources(_disabledPackageSources);
        }
    }
}
