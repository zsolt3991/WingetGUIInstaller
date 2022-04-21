using CommunityToolkit.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingetGUIInstaller.Constants;
using WingetHelper.Commands;
using WingetHelper.Models;

namespace WingetGUIInstaller.Services
{
    public class PackageCache
    {
        // Define a Treshold after which data is automatically fetched again
        private static readonly TimeSpan CacheValidityTreshold = TimeSpan.FromMinutes(5);

        private readonly ConsoleOutputCache _consoleBuffer;
        private readonly ApplicationDataStorageHelper _configurationStore;
        private List<WingetPackageEntry> _installedPackages;
        private List<WingetPackageEntry> _upgradablePackages;
        private DateTimeOffset _lastInstalledPackageRefresh;
        private DateTimeOffset _lastUpgrablePackageRefresh;

        public PackageCache(ConsoleOutputCache consoleOutputCache, ApplicationDataStorageHelper configurationStore)
        {
            _consoleBuffer = consoleOutputCache;
            _configurationStore = configurationStore;
        }

        private bool IsFilteringActive => _configurationStore
            .Read(ConfigurationPropertyKeys.PackageSourceFilteringEnabled, ConfigurationPropertyKeys.PackageSourceFilteringEnabledDefaultValue);

        private List<string> GetDisabledPackageSources() => _configurationStore
            .Read(ConfigurationPropertyKeys.DisabledPackageSources, ConfigurationPropertyKeys.DisabledPackageSourcesDefaultValue)
            .Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();

        public async Task<List<WingetPackageEntry>> GetInstalledPackages(bool forceReload = false)
        {
            if (_installedPackages == default || forceReload ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastInstalledPackageRefresh)
            {
                await LoadInstalledPackageList();
            }

            if (IsFilteringActive)
            {
                return _installedPackages
                    .Where(p => !GetDisabledPackageSources().Any(s => string.Equals(s, p.Source, StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();
            }

            return _installedPackages;
        }

        public async Task<List<WingetPackageEntry>> GetUpgradablePackages(bool forceReload)
        {
            if (_upgradablePackages == default || forceReload ||
                 DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastUpgrablePackageRefresh)
            {
                await LoadUpgradablePackages();
            }

            if (IsFilteringActive)
            {
                return _upgradablePackages
                    .Where(p => !GetDisabledPackageSources().Any(s => string.Equals(s, p.Source, StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();
            }

            return _upgradablePackages;
        }

        public async Task<List<WingetPackageEntry>> GetSearchResults(string searchQuery, bool refreshInstalled)
        {
            if (_installedPackages == default || refreshInstalled ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastInstalledPackageRefresh)
            {
                await LoadInstalledPackageList();
            }

            var searchResults = (await PackageCommands.SearchPackages(searchQuery)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage)
                .ExecuteAsync())
                .Where(r => !_installedPackages.Any(p => string.Equals(p.Id, r.Id, StringComparison.InvariantCultureIgnoreCase)))
                .ToList();

            if (IsFilteringActive)
            {
                return searchResults
                    .Where(p => !GetDisabledPackageSources().Any(s => string.Equals(s, p.Source, StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();
            }

            return searchResults;
        }


        private async Task LoadInstalledPackageList()
        {
            // Get all installed packages on the system
            _installedPackages = (await PackageCommands.GetInstalledPackages()
                .ConfigureOutputListener(_consoleBuffer.IngestMessage)
                .ExecuteAsync()).ToList();

            // Filter out the upgradable items as well to save one request so that all changes are accounted for
            _upgradablePackages = _installedPackages.FindAll(p => !string.IsNullOrWhiteSpace(p.Available));
            _lastInstalledPackageRefresh = DateTimeOffset.UtcNow;
            _lastUpgrablePackageRefresh = DateTimeOffset.UtcNow;
        }

        private async Task LoadUpgradablePackages()
        {
            // Get only packages that have upgrades available
            _upgradablePackages = (await PackageCommands.GetUpgradablePackages()
              .ConfigureOutputListener(_consoleBuffer.IngestMessage)
              .ExecuteAsync()).ToList();
            _lastUpgrablePackageRefresh = DateTimeOffset.UtcNow;
        }
    }
}
