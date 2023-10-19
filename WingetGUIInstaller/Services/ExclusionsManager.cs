using CommunityToolkit.Common.Extensions;
using CommunityToolkit.Helpers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using WingetGUIInstaller.Constants;

namespace WingetGUIInstaller.Services
{
    public class ExclusionsManager
    {
        private readonly ISettingsStorageHelper<string> _configurationStore;
        private readonly ILogger<ExclusionsManager> _logger;
        private List<string> _excludedPackageIds;
        private List<string> _excludedPackageSources;

        public ExclusionsManager(ISettingsStorageHelper<string> configurationStore, ILogger<ExclusionsManager> logger)
        {
            _configurationStore = configurationStore;
            _logger = logger;
        }

        public bool ExcludedPackagesEnabled => _configurationStore
            .GetValueOrDefault(ConfigurationPropertyKeys.ExcludedPackagesEnabled, ConfigurationPropertyKeys.ExcludedPackagesEnabledDefaultValue);

        public bool IgnoreEmptyPackageSourcesEnabled => _configurationStore
            .GetValueOrDefault(ConfigurationPropertyKeys.IgnoreEmptyPackageSources, ConfigurationPropertyKeys.IgnoreEmptyPackageSourcesDefaultValue);

        public bool ExcludedPackageSourcesEnabled => _configurationStore
            .GetValueOrDefault(ConfigurationPropertyKeys.PackageSourceFilteringEnabled, ConfigurationPropertyKeys.PackageSourceFilteringEnabledDefaultValue);

        public bool AddPackageExclusion(string exclusionId)
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = LoadPackageExclusionList();
            }
            if (!_excludedPackageIds.Contains(exclusionId))
            {
                _logger.LogInformation("Excluding package: {package}", exclusionId);
                _excludedPackageIds.Add(exclusionId);
                SavePackageExclusionList(_excludedPackageIds);
                return true;
            }
            return false;
        }

        public bool RemovePackageExclusion(string exclusionId)
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = LoadPackageExclusionList();
            }
            if (_excludedPackageIds.Contains(exclusionId))
            {
                _logger.LogInformation("Removing exclusion for package: {package}", exclusionId);
                _excludedPackageIds.Remove(exclusionId);
                SavePackageExclusionList(_excludedPackageIds);
                return true;
            }
            return false;
        }

        public bool IsPackageExcluded(string exclusionId, bool overrideFeatureEnabled = false)
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = LoadPackageExclusionList();
            }
            if (_excludedPackageIds.Contains(exclusionId))
            {
                return overrideFeatureEnabled || ExcludedPackagesEnabled;
            }
            return false;
        }

        public bool AddPackageSourceExclusion(string packageSourceName)
        {
            if (_excludedPackageSources == default)
            {
                _excludedPackageSources = LoadPackageSourceExclusionList();
            }
            if (!_excludedPackageSources.Contains(packageSourceName))
            {
                _logger.LogInformation("Excluding package source: {packageSource}", packageSourceName);
                _excludedPackageSources.Add(packageSourceName);
                SavePackageSourceExclusionList(_excludedPackageSources);
                return true;
            }
            return false;
        }

        public bool RemovePackageSourceExclusion(string packageSourceName)
        {
            if (_excludedPackageSources == default)
            {
                _excludedPackageSources = LoadPackageSourceExclusionList();
            }
            if (_excludedPackageSources.Contains(packageSourceName))
            {

                _logger.LogInformation("Removing exclusion for package source: {packageSource}", packageSourceName);
                _excludedPackageSources.Remove(packageSourceName);
                SavePackageSourceExclusionList(_excludedPackageIds);
                return true;
            }
            return false;
        }

        public bool IsPackageSourceExcluded(string packageSourceName, bool overrideFeatureEnabled = false)
        {
            if (_excludedPackageSources == default)
            {
                _excludedPackageSources = LoadPackageSourceExclusionList();
            }
            if (_excludedPackageSources.Contains(packageSourceName))
            {
                return overrideFeatureEnabled || ExcludedPackageSourcesEnabled;
            }
            return false;
        }

        private List<string> LoadPackageExclusionList()
        {
            var storedIds = _configurationStore.GetValueOrDefault(ConfigurationPropertyKeys.ExcludedPackageIds,
                ConfigurationPropertyKeys.ExcludedPackageIdsDefaultValue);

            if (string.IsNullOrEmpty(storedIds))
            {
                return new List<string>();
            }

            return JsonSerializer.Deserialize<List<string>>(storedIds);
        }

        private void SavePackageExclusionList(List<string> exclusionList)
        {
            var idsToStore = JsonSerializer.Serialize(exclusionList);
            _configurationStore.Save(ConfigurationPropertyKeys.ExcludedPackageIds, idsToStore);
        }

        private List<string> LoadPackageSourceExclusionList()
        {
            var storedSources = _configurationStore.GetValueOrDefault(ConfigurationPropertyKeys.DisabledPackageSources,
                ConfigurationPropertyKeys.DisabledPackageSourcesDefaultValue);

            if (string.IsNullOrEmpty(storedSources))
            {
                return new List<string>();
            }

            return JsonSerializer.Deserialize<List<string>>(storedSources);
        }

        private void SavePackageSourceExclusionList(List<string> packageSources)
        {
            var sourcesToStore = JsonSerializer.Serialize(packageSources);
            _configurationStore.Save(ConfigurationPropertyKeys.DisabledPackageSources, sourcesToStore);
        }
    }
}
