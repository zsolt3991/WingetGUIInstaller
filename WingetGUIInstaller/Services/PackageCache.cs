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

        private bool IgnoreEmptyPackageSource => _configurationStore
           .Read(ConfigurationPropertyKeys.IgnoreEmptyPackageSources, ConfigurationPropertyKeys.IgnoreEmptyPackageSourcesDefaultValue);

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

            IEnumerable<WingetPackageEntry> filteredPackages = _installedPackages;

            // Filter packages with no source
            if (IgnoreEmptyPackageSource)
            {
                filteredPackages = filteredPackages.Where(p => !string.IsNullOrWhiteSpace(p.Source));
            }

            // Filter Ignored Sources
            if (IsFilteringActive)
            {
                filteredPackages = filteredPackages
                    .Where(p => !GetDisabledPackageSources().Any(s => string.Equals(s, p.Source, StringComparison.InvariantCultureIgnoreCase)));
            }

            return filteredPackages.ToList();
        }

        public async Task<List<WingetPackageEntry>> GetUpgradablePackages(bool forceReload)
        {
            if (_upgradablePackages == default || forceReload ||
                 DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastUpgrablePackageRefresh)
            {
                await LoadUpgradablePackages();
            }

            IEnumerable<WingetPackageEntry> filteredPackages = _upgradablePackages;

            // Filter packages with no source
            if (IgnoreEmptyPackageSource)
            {
                filteredPackages = filteredPackages.Where(p => !string.IsNullOrWhiteSpace(p.Source));
            }

            // Filter Ignored Sources
            if (IsFilteringActive)
            {
                filteredPackages = filteredPackages
                    .Where(p => !GetDisabledPackageSources().Any(s => string.Equals(s, p.Source, StringComparison.InvariantCultureIgnoreCase)));
            }

            return filteredPackages.ToList();
        }

        public async Task<List<WingetPackageEntry>> GetSearchResults(string searchQuery, bool refreshInstalled)
        {
            if (_installedPackages == default || refreshInstalled ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastInstalledPackageRefresh)
            {
                await LoadInstalledPackageList();
            }

            var searchResults = await PackageCommands.SearchPackages(searchQuery)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage)
                .ExecuteAsync();

            // Ignore Packages already installed
            searchResults = searchResults
                .Where(r => !_installedPackages.Any(p => string.Equals(p.Id, r.Id, StringComparison.InvariantCultureIgnoreCase)));

            // Filter Ignored Sources. No need to check for empty sources here.
            if (IsFilteringActive)
            {
                searchResults = searchResults
                    .Where(p => !GetDisabledPackageSources().Any(s => string.Equals(s, p.Source, StringComparison.InvariantCultureIgnoreCase)));
            }

            return searchResults.ToList();
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
