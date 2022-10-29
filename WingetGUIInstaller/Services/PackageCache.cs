using CommunityToolkit.WinUI.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingetGUIInstaller.Constants;
using WingetHelper.Commands;
using WingetHelper.Models;

namespace WingetGUIInstaller.Services
{
    public sealed class PackageCache
    {
        private const int DetailsCacheSize = 5;
        // Define a Treshold after which data is automatically fetched again
        private static readonly TimeSpan CacheValidityTreshold = TimeSpan.FromMinutes(5);

        private readonly ConsoleOutputCache _consoleBuffer;
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly ToastNotificationManager _notificationManager;
        private readonly ExclusionsManager _exclusionsManager;
        private readonly ConcurrentQueue<QueueElement> _packageDetailsCache;
        private List<WingetPackageEntry> _installedPackages;
        private List<WingetPackageEntry> _upgradablePackages;
        private DateTimeOffset _lastInstalledPackageRefresh;
        private DateTimeOffset _lastUpgrablePackageRefresh;

        public PackageCache(ConsoleOutputCache consoleOutputCache, ApplicationDataStorageHelper configurationStore,
            ToastNotificationManager notificationManager, ExclusionsManager exclusionsManager)
        {
            _consoleBuffer = consoleOutputCache;
            _configurationStore = configurationStore;
            _notificationManager = notificationManager;
            _exclusionsManager = exclusionsManager;
            _packageDetailsCache = new ConcurrentQueue<QueueElement>();
        }

        private bool IsFilteringActive => _configurationStore
            .Read(ConfigurationPropertyKeys.PackageSourceFilteringEnabled, ConfigurationPropertyKeys.PackageSourceFilteringEnabledDefaultValue);

        private bool IgnoreEmptyPackageSource => _configurationStore
           .Read(ConfigurationPropertyKeys.IgnoreEmptyPackageSources, ConfigurationPropertyKeys.IgnoreEmptyPackageSourcesDefaultValue);

        private List<string> GetDisabledPackageSources() => _configurationStore
            .Read(ConfigurationPropertyKeys.DisabledPackageSources, ConfigurationPropertyKeys.DisabledPackageSourcesDefaultValue)?
            .Split(';', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

        public async Task<List<WingetPackageEntry>> GetInstalledPackages(bool forceReload = false, bool hideExcluded = true)
        {
            bool showNotification = false;
            if (_installedPackages == default || forceReload ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastInstalledPackageRefresh)
            {
                await LoadInstalledPackageList();
                showNotification = true;
            }

            IEnumerable<WingetPackageEntry> filteredPackages = _installedPackages ?? new List<WingetPackageEntry>();

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

            if (hideExcluded)
            {
                filteredPackages = filteredPackages.Where(p => !_exclusionsManager.IsExcluded(p.Id));
            }

            return filteredPackages.ToList();
        }

        public async Task<List<WingetPackageEntry>> GetUpgradablePackages(bool forceReload = false, bool hideExcluded = true)
        {
            bool showNotification = false;
            if (_upgradablePackages == default || forceReload ||
                 DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastUpgrablePackageRefresh)
            {
                await LoadUpgradablePackages();
                showNotification = true;
            }

            IEnumerable<WingetPackageEntry> filteredPackages = _upgradablePackages ?? new List<WingetPackageEntry>();

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

            if (hideExcluded)
            {
                filteredPackages = filteredPackages.Where(p => !_exclusionsManager.IsExcluded(p.Id));
            }

            if (showNotification)
            {
                _notificationManager.ShowUpdateStatus(filteredPackages.Count());
            }

            return filteredPackages.ToList();
        }

        public async Task<List<WingetPackageEntry>> GetSearchResults(string searchQuery, bool forceReload = false, bool hideExcluded = true)
        {
            if (_installedPackages == default || forceReload ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastInstalledPackageRefresh)
            {
                await LoadInstalledPackageList();
            }

            var searchResults = await PackageCommands.SearchPackages(searchQuery)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage)
                .ExecuteAsync();

            if (searchResults == default || !searchResults.Any())
            {
                // Return empty result set
                return new List<WingetPackageEntry>();
            }

            // Ignore Packages already installed
            searchResults = searchResults
                .Where(r => !_installedPackages?.Any(p => string.Equals(p.Id, r.Id, StringComparison.InvariantCultureIgnoreCase)) ?? false);

            // Filter Ignored Sources. No need to check for empty sources here.
            if (IsFilteringActive)
            {
                searchResults = searchResults
                    .Where(p => !GetDisabledPackageSources().Any(s => string.Equals(s, p.Source, StringComparison.InvariantCultureIgnoreCase)));
            }

            if (hideExcluded)
            {
                searchResults = searchResults.Where(p => !_exclusionsManager.IsExcluded(p.Id));
            }

            return searchResults.ToList();
        }

        public async Task<WingetPackageDetails> GetPackageDetails(string packageId, bool forceReload = false)
        {
            var cachedPackage = _packageDetailsCache.FirstOrDefault(p => p.PackageId == packageId);
            if (cachedPackage != default)
            {
                if (forceReload || DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= cachedPackage.LastUpdated)
                {
                    return await LoadPackageDetailsAsync(packageId);
                }
                else
                {
                    return cachedPackage.PackageDetails;
                }
            }

            return await LoadPackageDetailsAsync(packageId);
        }

        private async Task<WingetPackageDetails> LoadPackageDetailsAsync(string packageId)
        {
            var details = await PackageCommands.GetPackageDetails(packageId)
               .ConfigureOutputListener(_consoleBuffer.IngestMessage)
               .ExecuteAsync();

            if (details == default)
            {
                return default;
            }

            if (_packageDetailsCache.Count >= DetailsCacheSize)
            {
                var cachedPackage = _packageDetailsCache.FirstOrDefault(p => p.PackageId == packageId);
                if (cachedPackage != default)
                {
                    cachedPackage.PackageDetails = details;
                    cachedPackage.LastUpdated = DateTimeOffset.UtcNow;
                }
                else
                {
                    _packageDetailsCache.TryDequeue(out _);
                    _packageDetailsCache.Enqueue(new QueueElement
                    {
                        PackageId = packageId,
                        PackageDetails = details,
                        LastUpdated = DateTimeOffset.UtcNow
                    });
                }
            }
            else
            {
                _packageDetailsCache.Enqueue(new QueueElement
                {
                    PackageId = packageId,
                    PackageDetails = details,
                    LastUpdated = DateTimeOffset.UtcNow
                });
            }

            return details;
        }

        private async Task LoadInstalledPackageList()
        {
            // Get all installed packages on the system
            var commandResult = await PackageCommands.GetInstalledPackages()
                .ConfigureOutputListener(_consoleBuffer.IngestMessage)
                .ExecuteAsync();
            _installedPackages = commandResult != default ? commandResult.ToList() : new List<WingetPackageEntry>();

            // Filter out the upgradable items as well to save one request so that all changes are accounted for
            _upgradablePackages = _installedPackages.FindAll(p => !string.IsNullOrWhiteSpace(p.Available));
            _lastInstalledPackageRefresh = DateTimeOffset.UtcNow;
            _lastUpgrablePackageRefresh = DateTimeOffset.UtcNow;
        }

        private async Task LoadUpgradablePackages()
        {
            // Get only packages that have upgrades available
            var commandResult = await PackageCommands.GetUpgradablePackages()
              .ConfigureOutputListener(_consoleBuffer.IngestMessage)
              .ExecuteAsync();

            _upgradablePackages = commandResult != default ? commandResult.ToList() : new List<WingetPackageEntry>();
            _lastUpgrablePackageRefresh = DateTimeOffset.UtcNow;
        }

        private class QueueElement
        {
            public string PackageId { get; init; }
            public DateTimeOffset LastUpdated { get; set; }
            public WingetPackageDetails PackageDetails { get; set; }
        }
    }
}
