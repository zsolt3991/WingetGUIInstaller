using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Services;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class ExcludedPackagesViewModel : ObservableObject
    {
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly PackageCache _packageCache;
        private bool? _excludedPackagesEnabled;

        [ObservableProperty]
        private List<WingetPackageViewModel> _testList;

        public ExcludedPackagesViewModel(ApplicationDataStorageHelper configurationStore,
            PackageCache packageCache)
        {
            _configurationStore = configurationStore;
            _packageCache = packageCache;
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

        private async Task LoadExcludedPackagesAsync()
        {
            var packages = await _packageCache.GetInstalledPackages();
            _testList = packages.Select(p => new WingetPackageViewModel(p)).ToList();
        }
    }
}
