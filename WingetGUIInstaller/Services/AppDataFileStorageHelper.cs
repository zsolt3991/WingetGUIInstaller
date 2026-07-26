using CommunityToolkit.Common.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WingetGUIInstaller.Services
{
    internal sealed class AppDataFileStorageHelper : IFileStorageHelper
    {
        private readonly string _basePath;

        public AppDataFileStorageHelper(IApplicationDataProvider applicationDataProvider)
        {
            _basePath = applicationDataProvider.GetApplicationData().LocalPath;
            Directory.CreateDirectory(_basePath);
        }

        public async Task CreateFileAsync<T>(string filePath, T value)
        {
            var completePath = Path.Combine(_basePath, filePath);
            ValidatePath(completePath);
            var parentDirectory = Path.GetDirectoryName(completePath);
            if (!string.IsNullOrWhiteSpace(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            var fileContent = JsonSerializer.Serialize(value);
            await File.WriteAllTextAsync(completePath, fileContent);
        }

        public Task CreateFolderAsync(string folderPath)
        {
            var completePath = Path.Combine(_basePath, folderPath);
            ValidatePath(completePath);

            Directory.CreateDirectory(completePath);
            return Task.CompletedTask;
        }

        public async Task<T> ReadFileAsync<T>(string filePath, T defaultValue = default)
        {
            var completePath = Path.Combine(_basePath, filePath);
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
            var completePath = Path.Combine(_basePath, folderPath);
            ValidatePath(completePath);

            var resultSet = new List<(DirectoryItemType, string)>();

            if (!Directory.Exists(completePath))
            {
                return Task.FromResult<IEnumerable<(DirectoryItemType ItemType, string Name)>>(resultSet);
            }

            foreach (var subDirectory in Directory.GetDirectories(completePath))
            {
                resultSet.Add(new(DirectoryItemType.Folder, Path.GetFileName(subDirectory)));
            }

            foreach (var file in Directory.GetFiles(completePath))
            {
                resultSet.Add(new(DirectoryItemType.File, Path.GetFileName(file)));
            }

            return Task.FromResult<IEnumerable<(DirectoryItemType ItemType, string Name)>>(resultSet);
        }

        public Task<bool> TryDeleteItemAsync(string itemPath)
        {
            var completePath = Path.Combine(_basePath, itemPath);
            ValidatePath(completePath);

            if (Directory.Exists(completePath))
            {
                try
                {
                    Directory.Delete(completePath);
                    return Task.FromResult(true);
                }
                catch (IOException)
                {
                    return Task.FromResult(false);
                }
                catch (UnauthorizedAccessException)
                {
                    return Task.FromResult(false);
                }
            }

            if (File.Exists(completePath))
            {
                try
                {
                    File.Delete(completePath);
                    return Task.FromResult(true);
                }
                catch (IOException)
                {
                    return Task.FromResult(false);
                }
                catch (UnauthorizedAccessException)
                {
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(false);
        }

        public Task<bool> TryRenameItemAsync(string itemPath, string newName)
        {
            var oldPath = Path.Combine(_basePath, itemPath);
            var parentPath = Path.GetDirectoryName(oldPath) ?? _basePath;
            var newPath = Path.Combine(parentPath, newName);
            ValidatePath(oldPath);
            ValidatePath(newPath);

            if (Directory.Exists(oldPath))
            {
                try
                {
                    Directory.Move(oldPath, newPath);
                    return Task.FromResult(true);
                }
                catch (IOException)
                {
                    return Task.FromResult(false);
                }
                catch (UnauthorizedAccessException)
                {
                    return Task.FromResult(false);
                }
            }

            if (File.Exists(oldPath))
            {
                try
                {
                    File.Move(oldPath, newPath);
                    return Task.FromResult(true);
                }
                catch (IOException)
                {
                    return Task.FromResult(false);
                }
                catch (UnauthorizedAccessException)
                {
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(false);
        }

        private void ValidatePath(string path)
        {
            var basePath = Path.GetFullPath(_basePath);
            var completePath = Path.GetFullPath(path);

            if (!completePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Accessing a path outside the application directory is forbidden");
            }
        }
    }
}
