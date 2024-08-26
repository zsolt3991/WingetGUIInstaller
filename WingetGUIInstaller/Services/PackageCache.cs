using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WingetHelper.Commands;
using WingetHelper.Enums;
using WingetHelper.Models;
using WingetHelper.Services;

namespace WingetGUIInstaller.Services
{
    public sealed partial class PackageCache
    {
        // Define a Treshold after which data is automatically fetched again
        private static readonly TimeSpan CacheValidityTreshold = TimeSpan.FromMinutes(5);
        private const string FilterRegex = @"\[(?<tag>\S+)]\s*(?<value>\S+)\s*";

        private readonly ConsoleOutputCache _consoleBuffer;
        private readonly ExclusionsManager _exclusionsManager;
        private readonly ILogger<PackageCache> _logger;
        private readonly ICommandExecutor _commandExecutor;
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
            ILogger<PackageCache> logger)
        {
            _consoleBuffer = consoleOutputCache;
            _exclusionsManager = exclusionsManager;
            _logger = logger;
            _commandExecutor = commandExecutor;
            _installedCacheValidity = false;
            _updateCacheValidity = false;
        }

        public void InvalidateCache()
        {
            _logger.LogInformation("Package cache invalidated");
            _installedCacheValidity = false;
            _updateCacheValidity = false;
        }

        public async Task<List<WingetPackageEntry>> GetInstalledPackages(bool forceReload = false,
            bool ignoreSourceExclusion = false, bool ignorePackageExclusion = false)
        {
            if (_installedPackages == default || forceReload || !_installedCacheValidity ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastInstalledPackageRefresh)
            {
                _logger.LogInformation("Installed package cache refresh required. Force: {Force}", forceReload);
                await LoadInstalledPackageList();
            }

            if (_installedPackages == default || _installedPackages.Count == 0)
            {
                _logger.LogWarning("No installed packages available");
                return new List<WingetPackageEntry>();
            }

            return _installedPackages.FindAll(package => !IsPackageExcuded(package, ignoreSourceExclusion, ignorePackageExclusion));
        }

        public async Task<List<WingetPackageEntry>> GetUpgradablePackages(bool forceReload = false,
            bool ignoreSourceExclusion = false, bool ignorePackageExclusion = false)
        {
            if (_upgradablePackages == default || forceReload || !_updateCacheValidity ||
                 DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastUpgrablePackageRefresh)
            {
                _logger.LogInformation("Upgradable package cache refresh required. Force: {Force}", forceReload);
                await LoadUpgradablePackages();
            }

            if (_upgradablePackages == default || _upgradablePackages.Count == 0)
            {
                _logger.LogWarning("No upgragadable packages available");
                return new List<WingetPackageEntry>();
            }

            return _upgradablePackages.FindAll(package => !IsPackageExcuded(package, ignoreSourceExclusion, ignorePackageExclusion));
        }

        public async Task<List<WingetPackageEntry>> GetSearchResults(string searchQuery, bool forceReload = false,
            bool ignoreSourceExclusion = false, bool ignorePackageExclusion = false)
        {
            if (_installedPackages == default || forceReload || !_installedCacheValidity ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastInstalledPackageRefresh)
            {
                _logger.LogInformation("Installed package cache refresh required. Force: {Force}", forceReload);
                await LoadInstalledPackageList();
            }

            if (_searchResults == default || forceReload || _lastSerarchQuery != searchQuery ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastSearchTime)
            {
                await PerformSearchAsync(searchQuery);
            }

            if (_searchResults == default || _searchResults.Count == 0)
            {
                // Return empty result set
                _logger.LogWarning("No search results for query: {Query}", searchQuery);
                return new List<WingetPackageEntry>();
            }

            // Ignore Packages already installed
            return _searchResults.FindAll(r => !IsPackageInstalled(r) && !IsPackageExcuded(r, ignoreSourceExclusion, ignorePackageExclusion));
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
            _logger.LogInformation("Installed package cache refreshed at: {Time} count: {Count}",
                _lastUpgrablePackageRefresh, _installedPackages.Count);
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
            _logger.LogInformation("Upgradable package cache refreshed at: {Time} count: {Count}",
                _lastUpgrablePackageRefresh, _upgradablePackages.Count);
        }

        private async Task PerformSearchAsync(string searchQuery)
        {
            var searchArguments = ParseSearchQuery(searchQuery, out var remainingQuery);
            // Get all search results for the query
            var command = PackageCommands.SearchPackages(remainingQuery.Trim(), searchArguments)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            var commandResult = await _commandExecutor.ExecuteCommandAsync(command);

            _searchResults = commandResult != default ? commandResult.ToList() : new List<WingetPackageEntry>();
            _lastSearchTime = DateTimeOffset.UtcNow;
            _lastSerarchQuery = searchQuery;
            _logger.LogInformation("Installed search result cache refreshed at: {Time} count {Count}",
                _lastSearchTime, _searchResults.Count);
        }

        private bool IsPackageInstalled(WingetPackageEntry wingetPackageEntry)
        {
            if (_installedPackages == default)
            {
                return false;
            }

            return _installedPackages.Exists(p => string.Equals(p.Id, wingetPackageEntry.Id, StringComparison.InvariantCultureIgnoreCase));
        }

        private bool IsPackageExcuded(WingetPackageEntry wingetPackageEntry, bool ignoreSourceExclusion, bool ignorePackageExclusion)
        {
            if (!ignoreSourceExclusion)
            {
                // Filter packages with no source
                if (_exclusionsManager.IgnoreEmptyPackageSourcesEnabled)
                {
                    return string.IsNullOrWhiteSpace(wingetPackageEntry.Source);
                }

                // Filter Ignored Sources
                if (_exclusionsManager.ExcludedPackageSourcesEnabled)
                {
                    return _exclusionsManager.IsPackageSourceExcluded(wingetPackageEntry.Source);
                }
            }

            // Filter Ignored Packages
            if (!ignorePackageExclusion && _exclusionsManager.ExcludedPackagesEnabled &&
                _exclusionsManager.IsPackageExcluded(wingetPackageEntry.Id))
            {
                return true;
            }

            return false;
        }

        private static Dictionary<PackageFilterCriteria, string> ParseSearchQuery(string searchQuery, out string strippedQuery)
        {
            var parsedEntries = new Dictionary<PackageFilterCriteria, string>();
            foreach (var matchGroups in SearchQueryRegex().Matches(searchQuery).Select(m => m.Groups))
            {
                var tag = TagToFilterCriteria(matchGroups["tag"].Value);
                var value = matchGroups["value"].Value;
                if (tag != PackageFilterCriteria.Default)
                {
                    parsedEntries.Add(tag, value);
                }
            }
            strippedQuery = SearchQueryRegex().Replace(searchQuery, string.Empty);
            return parsedEntries;
        }

        private static PackageFilterCriteria TagToFilterCriteria(string tag)
        {
            return tag.ToLowerInvariant() switch
            {
                "tag" => PackageFilterCriteria.ByTag,
                "moniker" => PackageFilterCriteria.ByMoniker,
                "command" => PackageFilterCriteria.ByCommands,
                "name" => PackageFilterCriteria.ByName,
                "id" => PackageFilterCriteria.ById,
                _ => PackageFilterCriteria.Default,
            };
        }

        [GeneratedRegex(FilterRegex)]
        private static partial Regex SearchQueryRegex();
    }
}
