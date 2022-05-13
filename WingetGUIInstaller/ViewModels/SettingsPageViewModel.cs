using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using CommunityToolkit.WinUI.UI;
using GithubPackageUpdater.Services;
using Microsoft.UI.Dispatching;
using System;
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

namespace WingetGUIInstaller.ViewModels
{
    public class SettingsPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ConsoleOutputCache _cache;
        private readonly PackageSourceManager _packageSourceManager;
        private readonly PackageSourceCache _packageSourceCache;
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly GithubPackageUpdaterSerivce _updaterSerivce;
        private readonly List<string> _disabledPackageSources;
        private readonly ObservableCollection<WingetPackageSourceViewModel> _packageSources;
        private WingetPackageSourceViewModel _selectedSource;
        private string _newPackageSourceName;
        private string _newPackageSourceUrl;
        private bool? _consoleTabEnabled;
        private bool? _notificationsEnabled;
        private bool? _packageSourceFilteringEnabled;
        private bool? _ignoreEmptyPackageSourceEnabled;
        private bool? _automaticUpdatesEnabled;
        private AdvancedCollectionView _packageSourcesView;

        public SettingsPageViewModel(DispatcherQueue dispatcherQueue, ApplicationDataStorageHelper configurationStore,
           GithubPackageUpdaterSerivce updaterSerivce, ConsoleOutputCache cache,
           PackageSourceManager packageSourceManager, PackageSourceCache packageSourceCache)
        {
            _dispatcherQueue = dispatcherQueue;
            _configurationStore = configurationStore;
            _updaterSerivce = updaterSerivce;
            _cache = cache;
            _packageSourceManager = packageSourceManager;
            _packageSourceCache = packageSourceCache;
            _disabledPackageSources = LoadDisabledPackageSources();

            _packageSources = new ObservableCollection<WingetPackageSourceViewModel>();
            _packageSources.CollectionChanged += PackageSources_CollectionChanged;
            PackageSourcesView = new AdvancedCollectionView(_packageSources, true);

            _ = LoadPackageSourcesAsync();
        }

        public AdvancedCollectionView PackageSourcesView
        {
            get => _packageSourcesView;
            set => SetProperty(ref _packageSourcesView, value);
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

        public int SelectedCount => _packageSources.Any(p => p.IsSelected) ?
            _packageSources.Count(p => p.IsSelected) : SelectedSource != default ? 1 : 0;

        public ICommand AddPackageSourceCommand => new AsyncRelayCommand(()
            => AddPackageSourceAsync(NewPackageSourceName, NewPackageSourceUrl));

        public ICommand RemoveSelectedSourcesCommand => new AsyncRelayCommand(()
            => RemovePackageSourcesAsync(_packageSources.Where(p => p.IsSelected).Select(p => p.Name)));

        public ICommand CheckForUpdatesCommand => new AsyncRelayCommand(CheckForUpdatesAsync);

        private async Task CheckForUpdatesAsync()
        {
            var checkResult = await _updaterSerivce.CheckForUpdates(Package.Current);
            if (!checkResult.IsPackageUpToDate)
            {
                WeakReferenceMessenger.Default.Send(new UpdateAvailableMessage(checkResult));
            }
        }

        private async Task LoadPackageSourcesAsync(bool forceReload = false)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                _packageSources.Clear();
            });

            var wingetSources = await _packageSourceCache.GetAvailablePackageSources(forceReload);
            foreach (var entry in wingetSources)
            {
                _dispatcherQueue.TryEnqueue(() => _packageSources.Add(new WingetPackageSourceViewModel
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

            await _packageSourceManager.AddPackageSource(name, argument);
            await LoadPackageSourcesAsync(true);
        }

        private async Task RemovePackageSourcesAsync(IEnumerable<string> sourceNames)
        {
            if (sourceNames.Any())
            {
                foreach (var sourceName in sourceNames)
                {
                    await _packageSourceManager.RemovePackageSource(sourceName);
                }
                await LoadPackageSourcesAsync(true);
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
            foreach (var packageSource in _packageSources.Where(p => !p.IsEnabled))
            {
                if (_disabledPackageSources.Contains(packageSource.Name))
                    continue;
                _disabledPackageSources.Add(packageSource.Name);
            }
            SaveDisabledPackageSources(_disabledPackageSources);
        }
    }
}
