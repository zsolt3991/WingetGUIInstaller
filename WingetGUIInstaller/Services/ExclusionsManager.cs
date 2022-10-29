using CommunityToolkit.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
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

        public async Task<List<string>> GetExclusionsAsync()
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = await LoadExclusionListAsync().ConfigureAwait(false);
            }
            return _excludedPackageIds;
        }

        public List<string> GetExclusions()
        {
            return GetExclusionsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<bool> AddExclusionAsync(string exclusionId)
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = await LoadExclusionListAsync().ConfigureAwait(false);
            }
            if (!_excludedPackageIds.Contains(exclusionId))
            {
                _excludedPackageIds.Add(exclusionId);
                await SaveExclusionListAsync(_excludedPackageIds);
                return true;
            }
            return false;
        }

        public bool AddExclusion(string exclusionId)
        {
            return AddExclusionAsync(exclusionId).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<bool> RemoveExclusionAsync(string exclusionId)
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = await LoadExclusionListAsync().ConfigureAwait(false);
            }
            if (_excludedPackageIds.Contains(exclusionId))
            {
                _excludedPackageIds.Remove(exclusionId);
                await SaveExclusionListAsync(_excludedPackageIds);
                return true;
            }
            return false;
        }

        public bool RemoveExclusion(string exclusionId)
        {
            return RemoveExclusionAsync(exclusionId).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<bool> IsExcludedAsync(string exclusionId)
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = await LoadExclusionListAsync().ConfigureAwait(false);
            }
            if (_excludedPackageIds.Contains(exclusionId))
            {
                return ExcludedPackagesEnabled;
            }
            return false;
        }

        public bool IsExcluded(string exclusionId)
        {
            return IsExcludedAsync(exclusionId).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static async Task<List<string>> LoadExclusionListAsync()
        {
            if (!await StorageFileHelper.FileExistsAsync(ApplicationData.Current.LocalFolder,
                ConfigurationPropertyKeys.ExcludedPackagesFileName).ConfigureAwait(false))
            {
                return new List<string>();
            }
            var excludedPackagesFileContent = await StorageFileHelper
                .ReadTextFromLocalFileAsync(ConfigurationPropertyKeys.ExcludedPackagesFileName).ConfigureAwait(false);
            return JsonSerializer.Deserialize<List<string>>(excludedPackagesFileContent);
        }

        private static async Task SaveExclusionListAsync(List<string> exclusionList)
        {
            var excludedPackagesFileContent = JsonSerializer.Serialize(exclusionList);
            await StorageFileHelper.WriteTextToLocalFileAsync(excludedPackagesFileContent,
                ConfigurationPropertyKeys.ExcludedPackagesFileName, CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);
        }
    }
}
