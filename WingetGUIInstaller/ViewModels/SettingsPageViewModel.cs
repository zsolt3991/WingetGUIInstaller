﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetHelper.Commands;

namespace WingetGUIInstaller.ViewModels
{
    public class SettingsPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ConfigurationStore _configurationStore;
        private readonly ConsoleOutputCache _cache;
        private readonly List<string> _disabledPackageSources;
        private ObservableCollection<WingetPackageSourceViewModel> _packageSources;
        private WingetPackageSourceViewModel _selectedSource;
        private string _newPackageSourceName;
        private string _newPackageSourceUrl;
        private bool? _consoleTabEnabled;
        private bool? _notificationsEnabled;
        private bool? _packageSourceFilteringEnabled;

        public SettingsPageViewModel(DispatcherQueue dispatcherQueue, ConfigurationStore configurationStore, ConsoleOutputCache cache)
        {
            _dispatcherQueue = dispatcherQueue;
            _configurationStore = configurationStore;
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
                        .GetStoredProperty(ConfigurationPropertyKeys.ConsoleEnabled, true);
                    WeakReferenceMessenger.Default.Send(new ConsoleEnabledChangeMessage(_consoleTabEnabled.Value));
                }
                return _consoleTabEnabled.Value;
            }
            set
            {
                if (SetProperty(ref _consoleTabEnabled, value))
                {
                    _configurationStore.StoreProperty(ConfigurationPropertyKeys.ConsoleEnabled, value);
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
                        .GetStoredProperty(ConfigurationPropertyKeys.NotificationsEnabled, false);
                }
                return _notificationsEnabled.Value;
            }
            set
            {
                if (SetProperty(ref _notificationsEnabled, value))
                {
                    _configurationStore.StoreProperty(ConfigurationPropertyKeys.NotificationsEnabled, value);
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
                        .GetStoredProperty(ConfigurationPropertyKeys.PackageSourceFilteringEnabled, false);
                }
                return _packageSourceFilteringEnabled.Value;
            }
            set
            {
                if (SetProperty(ref _packageSourceFilteringEnabled, value))
                {
                    _configurationStore.StoreProperty(ConfigurationPropertyKeys.PackageSourceFilteringEnabled, value);
                }
            }
        }

        public List<string> EnabledPackageSources { get; set; }

        public int SelectedCount => PackageSources.Any(p => p.IsSelected) ?
            PackageSources.Count(p => p.IsSelected) : SelectedSource != default ? 1 : 0;

        public ICommand AddPackageSourceCommand => new AsyncRelayCommand(()
            => AddPackageSourceAsync(NewPackageSourceName, NewPackageSourceUrl));

        public ICommand RemoveSelectedSourcesCommand => new AsyncRelayCommand(()
            => RemovePackageSourcesAsync(PackageSources.Where(p => p.IsSelected).Select(p => p.Name)));

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
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    (item as WingetPackageSourceViewModel).PropertyChanged += OnPackagePropertyChanged;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in e.OldItems)
                {
                    (item as WingetPackageSourceViewModel).PropertyChanged -= OnPackagePropertyChanged;
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
                .GetStoredProperty(ConfigurationPropertyKeys.DisabledPackageSources, string.Empty);
            return serializedValue.Split(';', System.StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private void SaveDisabledPackageSources(List<string> packageSources)
        {
            var serializedValue = string.Join(';', packageSources);
            _configurationStore.StoreProperty(ConfigurationPropertyKeys.DisabledPackageSources, serializedValue);
        }

        private void UpdateEnabledPackageList()
        {
            foreach (var packageSource in PackageSources.Where(p => !p.IsEnabled))
            {
                if (_disabledPackageSources.Contains(packageSource.Name))
                    continue;
                _disabledPackageSources.Add(packageSource.Name);
                SaveDisabledPackageSources(_disabledPackageSources);
            }
        }
    }
}
