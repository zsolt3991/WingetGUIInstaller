using CommunityToolkit.WinUI.Helpers;
using System.Collections.Generic;
using System.Text.Json;
using WingetGUIInstaller.Constants;

namespace WingetGUIInstaller.Services
{
    public class ExclusionsManager
    {
        private readonly ApplicationDataStorageHelper _configurationStore;
        private List<string> _excludedPackageIds;
        private List<string> _excludedPackageSources;

        public ExclusionsManager(ApplicationDataStorageHelper configurationStore)
        {
            _configurationStore = configurationStore;
        }

        public bool ExcludedPackagesEnabled => _configurationStore
            .Read(ConfigurationPropertyKeys.ExcludedPackagesEnabled, ConfigurationPropertyKeys.ExcludedPackagesEnabledDefaultValue);

        public bool IgnoreEmptyPackageSourcesEnabled => _configurationStore
           .Read(ConfigurationPropertyKeys.IgnoreEmptyPackageSources, ConfigurationPropertyKeys.IgnoreEmptyPackageSourcesDefaultValue);

        public bool ExcludedPackageSourcesEnabled => _configurationStore
            .Read(ConfigurationPropertyKeys.PackageSourceFilteringEnabled, ConfigurationPropertyKeys.PackageSourceFilteringEnabledDefaultValue);

        public bool AddPackageExclusion(string exclusionId)
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = LoadPackageExclusionList();
            }
            if (!_excludedPackageIds.Contains(exclusionId))
            {
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
            var storedIds = _configurationStore.Read(ConfigurationPropertyKeys.ExcludedPackageIds,
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
            var storedSources = _configurationStore.Read(ConfigurationPropertyKeys.DisabledPackageSources,
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
