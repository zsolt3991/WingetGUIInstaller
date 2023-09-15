#if UNPACKAGED
using CommunityToolkit.Common.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WingetGUIInstaller.Constants;

namespace WingetGUIInstaller.Services
{
    internal sealed class UnPackagedFileStorageHelper : IFileStorageHelper
    {
        private readonly string _basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public async Task CreateFileAsync<T>(string filePath, T value)
        {
            var completePath = Path.Combine(_basePath, StorageFolderConstants.ApplicationFolderName, filePath);
            ValidatePath(completePath);

            var fileContent = JsonSerializer.Serialize<T>(value);
            await File.WriteAllTextAsync(completePath, fileContent);
        }

        public Task CreateFolderAsync(string folderPath)
        {
            var completePath = Path.Combine(_basePath, StorageFolderConstants.ApplicationFolderName, folderPath);
            ValidatePath(completePath);

            Directory.CreateDirectory(completePath);
            return Task.CompletedTask;
        }

        public async Task<T> ReadFileAsync<T>(string filePath, T defaultValue = default)
        {
            var completePath = Path.Combine(_basePath, StorageFolderConstants.ApplicationFolderName, filePath);
            ValidatePath(completePath);

            if (!File.Exists(completePath))
            {
                return defaultValue;
            }

            try
            {
                using (var fileStream = File.Open(completePath, FileMode.Open))
                {
                    return await JsonSerializer.DeserializeAsync<T>(fileStream).ConfigureAwait(false);
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        public Task<IEnumerable<(DirectoryItemType ItemType, string Name)>> ReadFolderAsync(string folderPath)
        {
            var completePath = Path.Combine(_basePath, StorageFolderConstants.ApplicationFolderName, folderPath);
            ValidatePath(completePath);

            var resultSet = new List<(DirectoryItemType, string)>();

            if (!Directory.Exists(completePath))
            {
                return Task.FromResult<IEnumerable<(DirectoryItemType ItemType, string Name)>>(resultSet);
            }

            foreach (var subDirectory in Directory.GetDirectories(completePath))
            {
                resultSet.Add(new(DirectoryItemType.Folder, subDirectory));
            }

            foreach (var file in Directory.GetFiles(completePath))
            {
                resultSet.Add(new(DirectoryItemType.File, file));
            }

            return Task.FromResult<IEnumerable<(DirectoryItemType ItemType, string Name)>>(resultSet);
        }

        public Task<bool> TryDeleteItemAsync(string itemPath)
        {
            var completePath = Path.Combine(_basePath, StorageFolderConstants.ApplicationFolderName, itemPath);
            ValidatePath(completePath);

            if (Directory.Exists(completePath))
            {
                try
                {
                    Directory.Delete(completePath);
                    return Task.FromResult(true);
                }
                catch
                {
                    return Task.FromResult(false);
                }
            }

            if (Directory.Exists(completePath))
            {
                try
                {
                    File.Delete(completePath);
                    return Task.FromResult(true);
                }
                catch
                {
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(false);
        }

        public Task<bool> TryRenameItemAsync(string itemPath, string newName)
        {
            var oldPath = Path.Combine(_basePath, StorageFolderConstants.ApplicationFolderName, itemPath);
            var newPath = Path.Combine(_basePath, StorageFolderConstants.ApplicationFolderName, newName);
            ValidatePath(oldPath);
            ValidatePath(newPath);

            if (Directory.Exists(oldPath))
            {
                try
                {
                    Directory.Move(oldPath, newPath);
                    return Task.FromResult(true);
                }
                catch
                {
                    return Task.FromResult(false);
                }
            }

            if (Directory.Exists(oldPath))
            {
                try
                {
                    File.Move(oldPath, newPath);
                    return Task.FromResult(true);
                }
                catch
                {
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(false);
        }

        private void ValidatePath(string path)
        {
            var basePath = Path.Combine(_basePath, StorageFolderConstants.ApplicationFolderName);
            var completePath = Path.GetFullPath(path);

            if (!completePath.StartsWith(basePath))
            {
                throw new InvalidOperationException("Accessing a path outside the application directory is forbidden");
            }
        }
    }
}
#endif