﻿using CommunityToolkit.Common.Extensions;
using CommunityToolkit.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class ExcludedPackagesViewModel : ObservableObject
    {
        private readonly ISettingsStorageHelper<string> _configurationStore;
        private readonly PackageCache _packageCache;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ExclusionsManager _exclusionsManager;
        private readonly ObservableCollection<WingetPackageViewModel> _exclusions;
        private readonly ObservableCollection<WingetPackageViewModel> _excludables;
        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForViewIndependentUse();
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

        public ExcludedPackagesViewModel(ISettingsStorageHelper<string> configurationStore,
            PackageCache packageCache, DispatcherQueue dispatcherQueue, ExclusionsManager exclusionsManager)
        {
            _configurationStore = configurationStore;
            _packageCache = packageCache;
            _dispatcherQueue = dispatcherQueue;
            _exclusionsManager = exclusionsManager;
            _loadingText = _resourceLoader.GetString("LoadingText");
            _exclusions = new ObservableCollection<WingetPackageViewModel>();
            _excludables = new ObservableCollection<WingetPackageViewModel>();
            _excludablePackagesCollection = new AdvancedCollectionView(_excludables, true);
            _excludedPackagesCollection = new AdvancedCollectionView(_exclusions, true);
            _ = LoadExcludedPackagesAsync();
        }

        public bool ExcludedPackagesEnabled
        {
            get => _excludedPackagesEnabled ??= _configurationStore
                .GetValueOrDefault(ConfigurationPropertyKeys.ExcludedPackagesEnabled, ConfigurationPropertyKeys.ExcludedPackagesEnabledDefaultValue);
            set
            {
                if (SetProperty(ref _excludedPackagesEnabled, value))
                {
                    _configurationStore.Save(ConfigurationPropertyKeys.ExcludedPackagesEnabled, value);
                    WeakReferenceMessenger.Default.Send(new ExclusionStatusChangedMessage(value));
                }
            }
        }

        [RelayCommand]
        private async Task AddExcludedPackage(WingetPackageViewModel package)
        {
            if (_exclusionsManager.AddPackageExclusion(package.Id))
            {
                WeakReferenceMessenger.Default.Send(new ExclusionListUpdatedMessage(true));
                await RebuildListsAsync();
            }
        }

        [RelayCommand]
        private async Task RemoveExcludedPackage(WingetPackageViewModel package)
        {
            if (_exclusionsManager.RemovePackageExclusion(package.Id))
            {
                WeakReferenceMessenger.Default.Send(new ExclusionListUpdatedMessage(false));
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
            var packages = await _packageCache.GetInstalledPackages(forceReload: forceRefresh,
                ignorePackageExclusion: true);

            _exclusions.Clear();
            foreach (var exclusion in packages
                .Where(package => _exclusionsManager.IsPackageExcluded(package.Id, true))
                .OrderBy(package => package.Name)
                .Select(package => new WingetPackageViewModel(package)))
            {
                _exclusions.Add(exclusion);
            }

            _excludables.Clear();
            foreach (var excludable in packages
               .Where(package => !_exclusionsManager.IsPackageExcluded(package.Id, true))
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
