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

        public ExclusionsManager(ApplicationDataStorageHelper configurationStore)
        {
            _configurationStore = configurationStore;
        }

        public bool ExcludedPackagesEnabled => _configurationStore
            .Read(ConfigurationPropertyKeys.ExcludedPackagesEnabled, ConfigurationPropertyKeys.ExcludedPackagesEnabledDefaultValue);

        public List<string> GetExclusions()
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = LoadPackageExclusionList();
            }
            return _excludedPackageIds;
        }

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

        public bool IsPackageExcluded(string exclusionId)
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = LoadPackageExclusionList();
            }
            if (_excludedPackageIds.Contains(exclusionId))
            {
                return ExcludedPackagesEnabled;
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
    }
}
