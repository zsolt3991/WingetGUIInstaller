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

        public async Task<List<string>> GetExclusions()
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = await LoadExclusionListAsync();
            }
            return _excludedPackageIds;
        }

        public async Task<bool> AddExclusionAsync(string exclusionId)
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = await LoadExclusionListAsync();
            }
            if (!_excludedPackageIds.Contains(exclusionId))
            {
                _excludedPackageIds.Add(exclusionId);
                await SaveExclusionListAsync(_excludedPackageIds);
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveExclusionAsync(string exclusionId)
        {
            if (_excludedPackageIds == default)
            {
                _excludedPackageIds = await LoadExclusionListAsync();
            }
            if (_excludedPackageIds.Contains(exclusionId))
            {
                _excludedPackageIds.Remove(exclusionId);
                await SaveExclusionListAsync(_excludedPackageIds);
                return true;
            }
            return false;
        }

        private static async Task<List<string>> LoadExclusionListAsync()
        {
            if (!await StorageFileHelper.FileExistsAsync(ApplicationData.Current.LocalFolder,
                ConfigurationPropertyKeys.ExcludedPackagesFileName))
            {
                return new List<string>();
            }
            var excludedPackagesFileContent = await StorageFileHelper
                .ReadTextFromLocalFileAsync(ConfigurationPropertyKeys.ExcludedPackagesFileName);
            return JsonSerializer.Deserialize<List<string>>(excludedPackagesFileContent);
        }

        private static async Task SaveExclusionListAsync(List<string> exclusionList)
        {
            var excludedPackagesFileContent = JsonSerializer.Serialize(exclusionList);
            await StorageFileHelper.WriteTextToLocalFileAsync(excludedPackagesFileContent,
                ConfigurationPropertyKeys.ExcludedPackagesFileName, CreationCollisionOption.ReplaceExisting);
        }
    }
}
