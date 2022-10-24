using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Services;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class ExcludedPackagesViewModel : ObservableObject
    {
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly PackageCache _packageCache;
        private readonly DispatcherQueue _dispatcherQueue;
        private bool? _excludedPackagesEnabled;
        private List<string> _excludedPackageIds;

        [ObservableProperty]
        private List<WingetPackageViewModel> _exclusionList;

        [ObservableProperty]
        private List<WingetPackageViewModel> _excludablePackageList;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _loadingText;

        public ExcludedPackagesViewModel(ApplicationDataStorageHelper configurationStore,
            PackageCache packageCache, DispatcherQueue dispatcherQueue)
        {
            _configurationStore = configurationStore;
            _packageCache = packageCache;
            _dispatcherQueue = dispatcherQueue;
            _loadingText = "Loading";
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
            if (!_excludedPackageIds.Contains(package.Id))
            {
                _excludedPackageIds.Add(package.Id);
                await SaveExclusionListAsync();
                await UpdatePackageListsAsync();
            }
        }

        [RelayCommand]
        private async Task RemoveExcludedPackage(WingetPackageViewModel package)
        {
            if (_excludedPackageIds.Contains(package.Id))
            {
                _excludedPackageIds.Remove(package.Id);
                await SaveExclusionListAsync();
                await UpdatePackageListsAsync();
            }
        }

        private async Task LoadExclusionListAsync()
        {
            if (!await StorageFileHelper.FileExistsAsync(ApplicationData.Current.LocalFolder,
                ConfigurationPropertyKeys.ExcludedPackagesFileName))
            {
                _excludedPackageIds = new List<string>();
                return;
            }
            var excludedPackagesFileContent = await StorageFileHelper
                .ReadTextFromLocalFileAsync(ConfigurationPropertyKeys.ExcludedPackagesFileName);
            _excludedPackageIds = JsonSerializer.Deserialize<List<string>>(excludedPackagesFileContent);
        }

        private async Task SaveExclusionListAsync()
        {
            var excludedPackagesFileContent = JsonSerializer.Serialize(_excludedPackageIds);
            await StorageFileHelper.WriteTextToLocalFileAsync(excludedPackagesFileContent,
                ConfigurationPropertyKeys.ExcludedPackagesFileName, CreationCollisionOption.ReplaceExisting);
        }

        private async Task LoadExcludedPackagesAsync()
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            await LoadExclusionListAsync();
            await UpdatePackageListsAsync();
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
        }

        private async Task UpdatePackageListsAsync(bool forceRefresh = false)
        {
            var packages = await _packageCache.GetInstalledPackages(forceRefresh);

            ExclusionList = packages
                .Where(package => _excludedPackageIds.Contains(package.Id))
                .Select(package => new WingetPackageViewModel(package))
                .ToList();

            ExcludablePackageList = packages
                .Where(package => !_excludedPackageIds.Contains(package.Id))
                .Select(package => new WingetPackageViewModel(package))
                .ToList();
        }
    }
}
