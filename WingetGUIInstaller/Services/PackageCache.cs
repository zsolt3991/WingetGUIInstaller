using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingetGUIInstaller.Utils;
using WingetHelper.Commands;
using WingetHelper.Models;
using WingetHelper.Services;

namespace WingetGUIInstaller.Services
{
    public sealed class PackageCache
    {
        private const int DetailsCacheSize = 5;
        // Define a Treshold after which data is automatically fetched again
        private static readonly TimeSpan CacheValidityTreshold = TimeSpan.FromMinutes(5);

        private readonly ConsoleOutputCache _consoleBuffer;
        private readonly ExclusionsManager _exclusionsManager;
        private readonly ILogger<PackageCache> _logger;
        private readonly ILoggerFactory _commandLoggerFactory;
        private readonly ICommandExecutor _commandExecutor;
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

        public PackageCache(ConsoleOutputCache consoleOutputCache, ExclusionsManager exclusionsManager, ICommandExecutor commandExecutor,
            ILogger<PackageCache> logger, ILoggerFactory commandLoggerFactory)
        {
            _consoleBuffer = consoleOutputCache;
            _exclusionsManager = exclusionsManager;
            _logger = logger;
            _commandLoggerFactory = commandLoggerFactory;
            _commandExecutor = commandExecutor;
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
                _logger.LogInformation("Installed package cache refresh required. Force: {force}", forceReload);
                await LoadInstalledPackageList();
            }

            if (_installedPackages == default || !_installedPackages.Any())
            {
                _logger.LogWarning("No installed packages available");
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
                _logger.LogInformation("Upgradable package cache refresh required. Force: {force}", forceReload);
                await LoadUpgradablePackages();
            }

            if (_upgradablePackages == default || !_upgradablePackages.Any())
            {
                _logger.LogWarning("No upgragadable packages available");
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
                _logger.LogInformation("Installed package cache refresh required. Force: {force}", forceReload);
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
                _logger.LogWarning("No search results for query: {query}", searchQuery);
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
                    _logger.LogInformation("Package details refresh required. Force: {force}", forceReload);
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
            var detailsCommand = PackageCommands.GetPackageDetails(packageId)
               .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            var details = await _commandExecutor.ExecuteCommandAsync(detailsCommand);

            if (details == default)
            {
                _logger.LogWarning("No package details for packageId: {packageId}", packageId);
                return default;
            }

            if (_packageDetailsCache.Count >= DetailsCacheSize)
            {
                var cachedPackage = _packageDetailsCache.FirstOrDefault(p => p.PackageId == packageId);
                if (cachedPackage != default)
                {
                    _logger.LogInformation("Updating cached package details for packageId: {packageId}", packageId);
                    cachedPackage.PackageDetails = details;
                    cachedPackage.LastUpdated = DateTimeOffset.UtcNow;
                }
                else
                {
                    _packageDetailsCache.TryDequeue(out var itemToRemove);
                    _logger.LogInformation("Removed cached package details for packageId: {packageId}", itemToRemove?.PackageId);
                    _logger.LogInformation("Adding package details to cache for packageId: {packageId}", packageId);
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
                _logger.LogInformation("Adding package details to cache for packageId: {packageId}", packageId);
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
            var command = PackageCommands.GetInstalledPackages()
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            var commandResult = await _commandExecutor.ExecuteCommandAsync(command);

            _installedPackages = commandResult != default ? commandResult.ToList() : new List<WingetPackageEntry>();
            _lastInstalledPackageRefresh = DateTimeOffset.UtcNow;
            _installedCacheValidity = true;
            _logger.LogInformation("Installed package cache refreshed at: {time}", _lastUpgrablePackageRefresh);
        }

        private async Task LoadUpgradablePackages()
        {
            // Get only packages that have upgrades available
            var command = PackageCommands.GetUpgradablePackages()
              .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            var commandResult = await _commandExecutor.ExecuteCommandAsync(command);

            _upgradablePackages = commandResult != default ? commandResult.ToList() : new List<WingetPackageEntry>();
            _lastUpgrablePackageRefresh = DateTimeOffset.UtcNow;
            _updateCacheValidity = true;
            _logger.LogInformation("Upgradable package cache refreshed at: {time}", _lastUpgrablePackageRefresh);
        }

        private async Task PerformSearchAsync(string searchQuery)
        {
            // Get all search results for the query
            var command = PackageCommands.SearchPackages(searchQuery)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            var commandResult = await _commandExecutor.ExecuteCommandAsync(command);

            _searchResults = commandResult != default ? commandResult.ToList() : new List<WingetPackageEntry>();
            _lastSearchTime = DateTimeOffset.UtcNow;
            _lastSerarchQuery = searchQuery;
            _logger.LogInformation("Installed search result cache refreshed at: {time}", _lastUpgrablePackageRefresh);
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
