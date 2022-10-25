using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class ExcludedPackagesViewModel : ObservableObject
    {
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly PackageCache _packageCache;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ExclusionsManager _exclusionsManager;
        private readonly ObservableCollection<WingetPackageViewModel> _exclusions;
        private readonly ObservableCollection<WingetPackageViewModel> _excludables;
        private bool? _excludedPackagesEnabled;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _loadingText;

        [ObservableProperty]
        private string _filterExcludedPackagesText;

        [ObservableProperty]
        private string _filterExcludablePackagesText;

        [ObservableProperty]
        private AdvancedCollectionView _excludedPackagesCollection;

        [ObservableProperty]
        private AdvancedCollectionView _excludablePackagesCollection;

        public ExcludedPackagesViewModel(ApplicationDataStorageHelper configurationStore,
            PackageCache packageCache, DispatcherQueue dispatcherQueue, ExclusionsManager exclusionsManager)
        {
            _configurationStore = configurationStore;
            _packageCache = packageCache;
            _dispatcherQueue = dispatcherQueue;
            _exclusionsManager = exclusionsManager;
            _loadingText = "Loading";
            _exclusions = new ObservableCollection<WingetPackageViewModel>();
            _excludables = new ObservableCollection<WingetPackageViewModel>();
            _excludablePackagesCollection = new AdvancedCollectionView(_excludables, true);
            _excludedPackagesCollection = new AdvancedCollectionView(_exclusions, true);
            _ = LoadExcludedPackagesAsync();
        }

        public bool ExcludedPackagesEnabled
        {
            get => _excludedPackagesEnabled ??= _configurationStore
                .Read(ConfigurationPropertyKeys.ExcludedPackagesEnabled, ConfigurationPropertyKeys.ExcludedPackagesEnabledDefaultValue);
            set
            {
                if (SetProperty(ref _excludedPackagesEnabled, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.ExcludedPackagesEnabled, value);
                }
            }
        }

        [RelayCommand]
        private async Task AddExcludedPackage(WingetPackageViewModel package)
        {
            if (await _exclusionsManager.AddExclusionAsync(package.Id))
            {
                await RebuildListsAsync();
            }
        }

        [RelayCommand]
        private async Task RemoveExcludedPackage(WingetPackageViewModel package)
        {
            if (await _exclusionsManager.RemoveExclusionAsync(package.Id))
            {
                await RebuildListsAsync();
            }
        }

        private async Task LoadExcludedPackagesAsync()
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            await RebuildListsAsync();
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
        }

        private async Task RebuildListsAsync(bool forceRefresh = false)
        {
            var packages = await _packageCache.GetInstalledPackages(forceRefresh);
            var excludedIds = await _exclusionsManager.GetExclusions();

            _exclusions.Clear();
            foreach (var exclusion in packages
                .Where(package => excludedIds.Contains(package.Id))
                .OrderBy(package => package.Name)
                .Select(package => new WingetPackageViewModel(package)))
            {
                _exclusions.Add(exclusion);
            }

            _excludables.Clear();
            foreach (var excludable in packages
               .Where(package => !excludedIds.Contains(package.Id))
               .OrderBy(package => package.Name)
               .Select(package => new WingetPackageViewModel(package)))
            {
                _excludables.Add(excludable);
            }
        }

        partial void OnFilterExcludedPackagesTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ExcludedPackagesCollection.ClearFiltering();
            }

            ExcludedPackagesCollection.ApplyFiltering<WingetPackageViewModel>(package =>
                package.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase)
                || package.Id.Contains(value, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        partial void OnFilterExcludablePackagesTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ExcludablePackagesCollection.ClearFiltering();
            }

            ExcludablePackagesCollection.ApplyFiltering<WingetPackageViewModel>(package =>
                package.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase)
                || package.Id.Contains(value, StringComparison.InvariantCultureIgnoreCase)
            );
        }
    }
}
