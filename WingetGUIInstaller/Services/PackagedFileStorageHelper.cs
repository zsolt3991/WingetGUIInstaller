#if !UNPACKAGED
using CommunityToolkit.Common.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace WingetGUIInstaller.Services
{
    internal sealed class PackagedFileStorageHelper : IFileStorageHelper
    {
        private readonly StorageFolder _rootFolder;

        public PackagedFileStorageHelper()
        {
            _rootFolder = ApplicationData.Current.LocalFolder;
        }

        public async Task CreateFileAsync<T>(string filePath, T value)
        {
            var completePath = Path.Combine(_rootFolder.Path, filePath);
            ValidatePath(completePath);

            var relativePath = Path.GetRelativePath(Path.GetFullPath(completePath), _rootFolder.Path);
            var file = await _rootFolder.CreateFileAsync(relativePath, CreationCollisionOption.ReplaceExisting);
            var fileContent = JsonSerializer.Serialize<T>(value);

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(fileContent);
                    await writer.FlushAsync();
                }
            }
        }

        public async Task CreateFolderAsync(string folderPath)
        {
            var completePath = Path.Combine(_rootFolder.Path, folderPath);
            ValidatePath(completePath);

            var relativePath = Path.GetRelativePath(Path.GetFullPath(completePath), _rootFolder.Path);
            await _rootFolder.CreateFolderAsync(relativePath, CreationCollisionOption.OpenIfExists);
        }

        public async Task<T> ReadFileAsync<T>(string filePath, T @default = default)
        {
            var completePath = Path.Combine(_rootFolder.Path, filePath);
            ValidatePath(completePath);

            var relativePath = Path.GetRelativePath(Path.GetFullPath(completePath), _rootFolder.Path);
            using (var fileStream = await _rootFolder.OpenStreamForReadAsync(relativePath))
            {
                return JsonSerializer.Deserialize<T>(fileStream);
            }
        }

        public async Task<IEnumerable<(DirectoryItemType ItemType, string Name)>> ReadFolderAsync(string folderPath)
        {
            var completePath = Path.Combine(_rootFolder.Path, folderPath);
            ValidatePath(completePath);

            var relativePath = Path.GetRelativePath(Path.GetFullPath(completePath), _rootFolder.Path);
            var baseFolder = await _rootFolder.GetFolderAsync(relativePath);

            var resultSet = new List<(DirectoryItemType, string)>();

            foreach (var subDirectory in await baseFolder.GetFoldersAsync())
            {
                resultSet.Add(new(DirectoryItemType.Folder, subDirectory.Name));
            }

            foreach (var file in await baseFolder.GetFilesAsync())
            {
                resultSet.Add(new(DirectoryItemType.File, file.Name));
            }

            return resultSet;
        }

        public async Task<bool> TryDeleteItemAsync(string itemPath)
        {
            var completePath = Path.Combine(_rootFolder.Path, itemPath);
            ValidatePath(completePath);

            var relativePath = Path.GetRelativePath(Path.GetFullPath(completePath), _rootFolder.Path);
            var item = await _rootFolder.GetItemAsync(relativePath);
            await item.DeleteAsync();
            return true;
        }

        public async Task<bool> TryRenameItemAsync(string itemPath, string newName)
        {
            var completePath = Path.Combine(_rootFolder.Path, itemPath);
            ValidatePath(completePath);

            var relativePath = Path.GetRelativePath(Path.GetFullPath(completePath), _rootFolder.Path);
            var item = await _rootFolder.GetItemAsync(relativePath);
            await item.RenameAsync(newName);
            return true;
        }

        private void ValidatePath(string path)
        {
            var basePath = Path.GetFullPath(_rootFolder.Path);
            var completePath = Path.GetFullPath(path);

            if (!completePath.StartsWith(basePath))
            {
                throw new InvalidOperationException("Accessing a path outside the application directory is forbidden");
            }
        }
    }
}
#endif