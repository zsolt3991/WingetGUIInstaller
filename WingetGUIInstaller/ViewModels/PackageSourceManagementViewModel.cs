using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;

namespace WingetGUIInstaller.ViewModels
{
    public class PackageSourceManagementViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly PackageSourceManager _packageSourceManager;
        private readonly PackageSourceCache _packageSourceCache;
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly List<string> _disabledPackageSources;
        private readonly ObservableCollection<WingetPackageSourceViewModel> _packageSources;

        private AdvancedCollectionView _packageSourcesView;
        private WingetPackageSourceViewModel _selectedSource;
        private string _newPackageSourceName;
        private string _newPackageSourceUrl;
        private string _filterText;
        private bool _isLoading;

        public PackageSourceManagementViewModel(DispatcherQueue dispatcherQueue, PackageSourceManager packageSourceManager, PackageSourceCache packageSourceCache, ApplicationDataStorageHelper configurationStore)
        {
            _dispatcherQueue = dispatcherQueue;
            _packageSourceManager = packageSourceManager;
            _packageSourceCache = packageSourceCache;
            _configurationStore = configurationStore;
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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    ApplyPackageSourceFilter(value);
                }
            }
        }

        public ICommand RefreshSourcesCommand => new AsyncRelayCommand(()
            => LoadPackageSourcesAsync(true));

        public ICommand AddPackageSourceCommand => new AsyncRelayCommand(()
            => AddPackageSourceAsync(NewPackageSourceName, NewPackageSourceUrl));

        public ICommand RemoveSelectedSourcesCommand => new AsyncRelayCommand(()
            => RemovePackageSourcesAsync(_packageSources.Where(p => p.IsSelected).Select(p => p.Name)));

        public int SelectedCount => _packageSources.Any(p => p.IsSelected) ?
            _packageSources.Count(p => p.IsSelected) : SelectedSource != default ? 1 : 0;

        private async Task LoadPackageSourcesAsync(bool forceReload = false)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                _packageSources.Clear();
                IsLoading = true;
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
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
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
            return serializedValue?.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
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

        private void ApplyPackageSourceFilter(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                PackageSourcesView.ClearFiltering();
            }

            PackageSourcesView.ApplyFiltering<WingetPackageSourceViewModel>(packageSource =>
                packageSource.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                || packageSource.Url.Contains(query, StringComparison.InvariantCultureIgnoreCase)
            );
        }
    }
}
