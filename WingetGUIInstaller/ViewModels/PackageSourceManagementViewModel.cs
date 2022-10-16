﻿using CommunityToolkit.Mvvm.ComponentModel;
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
    public sealed partial class PackageSourceManagementViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly PackageSourceManager _packageSourceManager;
        private readonly PackageSourceCache _packageSourceCache;
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly List<string> _disabledPackageSources;
        private readonly ObservableCollection<WingetPackageSourceViewModel> _packageSources;

        [ObservableProperty]
        private AdvancedCollectionView _packageSourcesView;

        [ObservableProperty]
        private string _newPackageSourceName;

        [ObservableProperty]
        private string _newPackageSourceUrl;

        [ObservableProperty]
        private string _filterText;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedCount))]
        [NotifyPropertyChangedFor(nameof(IsSomethingSelected))]
        [NotifyCanExecuteChangedFor(nameof(RemoveSelectedPackageSourcesCommand))]
        private WingetPackageSourceViewModel _selectedSource;

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

        public int SelectedCount => _packageSources.Any(p => p.IsSelected) ?
            _packageSources.Count(p => p.IsSelected) : SelectedSource != default ? 1 : 0;

        public bool IsSomethingSelected => SelectedCount > 0;

        [RelayCommand]
        private async Task RefreshPackageSourceList()
        {
            await LoadPackageSourcesAsync(true);
        }

        [RelayCommand]
        private async Task AddPackageSourceAsync()
        {
            if (string.IsNullOrEmpty(NewPackageSourceName) || string.IsNullOrEmpty(NewPackageSourceUrl))
            {
                return;
            }

            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            await _packageSourceManager.AddPackageSource(NewPackageSourceName, NewPackageSourceUrl);
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);

            await LoadPackageSourcesAsync(true);

        }

        [RelayCommand(CanExecute = nameof(IsSomethingSelected))]
        private async Task RemoveSelectedPackageSourcesAsync()
        {
            var sourceNames = GetSelectedPackageSourceNames();
            if (sourceNames.Any())
            {
                _dispatcherQueue.TryEnqueue(() => IsLoading = true);
                foreach (var sourceName in sourceNames)
                {
                    await _packageSourceManager.RemovePackageSource(sourceName);
                }
                _dispatcherQueue.TryEnqueue(() => IsLoading = false);

                await LoadPackageSourcesAsync(true);
            }
        }

        partial void OnFilterTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                PackageSourcesView.ClearFiltering();
            }

            PackageSourcesView.ApplyFiltering<WingetPackageSourceViewModel>(packageSource =>
                packageSource.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase)
                || packageSource.Argument.Contains(value, StringComparison.InvariantCultureIgnoreCase)
            );
        }

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
                _dispatcherQueue.TryEnqueue(() => _packageSources.Add(
                    new WingetPackageSourceViewModel(entry, !_disabledPackageSources.Contains(entry.Name))));
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
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
                    OnPropertyChanged(nameof(IsSomethingSelected));
                    RemoveSelectedPackageSourcesCommand.NotifyCanExecuteChanged();
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

            foreach (var packageSource in _packageSources.Where(p => p.IsEnabled))
            {
                if (!_disabledPackageSources.Contains(packageSource.Name))
                    continue;
                _disabledPackageSources.Remove(packageSource.Name);
            }

            SaveDisabledPackageSources(_disabledPackageSources);
        }

        private IEnumerable<string> GetSelectedPackageSourceNames()
        {
            // Prioritize Selected Items 
            if (_packageSources.Any(p => p.IsSelected))
            {
                return _packageSources.Where(p => p.IsSelected).Select(p => p.Name);
            }

            // Use the highlighted item if there is no selection
            if (_selectedSource != default)
            {
                return new List<string>() { _selectedSource.Name };
            }

            return new List<string>();
        }
    }
}
