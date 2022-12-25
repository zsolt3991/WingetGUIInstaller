using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly ExclusionsManager _exclusionsManager;
        private readonly ConcurrentQueue<QueueElement> _packageDetailsCache;
        private List<WingetPackageEntry> _installedPackages;
        private List<WingetPackageEntry> _upgradablePackages;
        private List<WingetPackageEntry> _searchResults;
        private DateTimeOffset _lastInstalledPackageRefresh;
        private DateTimeOffset _lastUpgrablePackageRefresh;
        private DateTimeOffset _lastSearchTime;
        private string _lastSerarchQuery;
        private bool _installedCacheValidity;
        private bool _updateCacheValidity;

        public PackageCache(ConsoleOutputCache consoleOutputCache, ExclusionsManager exclusionsManager)
        {
            _consoleBuffer = consoleOutputCache;
            _exclusionsManager = exclusionsManager;
            _packageDetailsCache = new ConcurrentQueue<QueueElement>();
            _installedCacheValidity = false;
            _updateCacheValidity = false;
        }

        public void InvalidateCache()
        {
            _installedCacheValidity = false;
            _updateCacheValidity = false;
        }

        public async Task<List<WingetPackageEntry>> GetInstalledPackages(bool forceReload = false,
            bool ignoreSourceExclusion = false, bool ignorePackageExclusion = false)
        {
            if (_installedPackages == default || forceReload || !_installedCacheValidity ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastInstalledPackageRefresh)
            {
                await LoadInstalledPackageList();
            }

            if (_installedPackages == default || !_installedPackages.Any())
            {
                return new List<WingetPackageEntry>();
            }

            var filteredPackages = ApplyExclusions(_installedPackages, ignoreSourceExclusion, ignorePackageExclusion);
            return filteredPackages.ToList();
        }

        public async Task<List<WingetPackageEntry>> GetUpgradablePackages(bool forceReload = false,
            bool ignoreSourceExclusion = false, bool ignorePackageExclusion = false)
        {
            if (_upgradablePackages == default || forceReload || !_updateCacheValidity ||
                 DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastUpgrablePackageRefresh)
            {
                await LoadUpgradablePackages();
            }

            if (_upgradablePackages == default || !_upgradablePackages.Any())
            {
                return new List<WingetPackageEntry>();
            }

            var filteredPackages = ApplyExclusions(_upgradablePackages, ignoreSourceExclusion, ignorePackageExclusion);
            return filteredPackages.ToList();
        }

        public async Task<List<WingetPackageEntry>> GetSearchResults(string searchQuery, bool forceReload = false,
            bool ignoreSourceExclusion = false, bool ignorePackageExclusion = false)
        {
            if (_installedPackages == default || forceReload || !_installedCacheValidity ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastInstalledPackageRefresh)
            {
                await LoadInstalledPackageList();
            }

            if (_searchResults == default || forceReload || _lastSerarchQuery != searchQuery ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastSearchTime)
            {
                await PerformSearchAsync(searchQuery);
            }

            if (_searchResults == default || !_searchResults.Any())
            {
                // Return empty result set
                return new List<WingetPackageEntry>();
            }

            // Ignore Packages already installed
            var searchResults = _searchResults
                .Where(r => !_installedPackages?.Any(p => string.Equals(p.Id, r.Id, StringComparison.InvariantCultureIgnoreCase)) ?? false);

            searchResults = ApplyExclusions(searchResults, ignoreSourceExclusion, ignorePackageExclusion);
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
            _lastInstalledPackageRefresh = DateTimeOffset.UtcNow;
            _installedCacheValidity = true;
        }

        private async Task LoadUpgradablePackages()
        {
            // Get only packages that have upgrades available
            var commandResult = await PackageCommands.GetUpgradablePackages()
              .ConfigureOutputListener(_consoleBuffer.IngestMessage)
              .ExecuteAsync();

            _upgradablePackages = commandResult != default ? commandResult.ToList() : new List<WingetPackageEntry>();
            _lastUpgrablePackageRefresh = DateTimeOffset.UtcNow;
            _updateCacheValidity = true;
        }

        private async Task PerformSearchAsync(string searchQuery)
        {
            // Get all search results for the query
            var commandResult = await PackageCommands.SearchPackages(searchQuery)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage)
                .ExecuteAsync();

            _searchResults = commandResult != default ? commandResult.ToList() : new List<WingetPackageEntry>();
            _lastSearchTime = DateTimeOffset.UtcNow;
            _lastSerarchQuery = searchQuery;
        }

        private IEnumerable<WingetPackageEntry> ApplyExclusions(IEnumerable<WingetPackageEntry> packages,
            bool ignoreSourceExclusion, bool ignorePackageExclusion)
        {
            // Filter packages with no source
            if (!ignoreSourceExclusion && _exclusionsManager.IgnoreEmptyPackageSourcesEnabled)
            {
                packages = packages.Where(p => !string.IsNullOrWhiteSpace(p.Source));
            }

            // Filter Ignored Sources
            if (!ignoreSourceExclusion && _exclusionsManager.ExcludedPackageSourcesEnabled)
            {
                packages = packages
                    .Where(p => !_exclusionsManager.IsPackageSourceExcluded(p.Source));
            }

            // Filter Ignored Packages
            if (!ignorePackageExclusion && _exclusionsManager.ExcludedPackagesEnabled)
            {
                packages = packages
                    .Where(p => !_exclusionsManager.IsPackageExcluded(p.Id));
            }

            return packages;
        }

        private sealed class QueueElement
        {
            public string PackageId { get; init; }
            public DateTimeOffset LastUpdated { get; set; }
            public WingetPackageDetails PackageDetails { get; set; }
        }
    }
}
