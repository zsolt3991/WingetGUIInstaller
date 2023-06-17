using CommunityToolkit.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WingetGUIInstaller.Services
{
    internal sealed class UnpackagedSettingStorageHelper : ISettingsStorageHelper<string>
    {
        private const string SettingsFileName = "settings.json";
        private readonly IFileStorageHelper _fileStorage;
        private readonly SemaphoreSlim _fileAccessSemaphore;
        private IDictionary<string, string> _settings;

        public UnpackagedSettingStorageHelper(IFileStorageHelper fileStorage)
        {
            _fileStorage = fileStorage;
            _settings = _fileStorage.ReadFileAsync(SettingsFileName, new Dictionary<string, string>()).GetAwaiter().GetResult();
            _fileAccessSemaphore = new SemaphoreSlim(1);
        }

        public void Clear()
        {
            if (!_fileStorage.TryDeleteItemAsync(SettingsFileName).GetAwaiter().GetResult())
            {
                throw new InvalidOperationException("Failed to clear application settings");
            };
        }

        public void Save<TValue>(string key, TValue value)
        {
            _settings.TryAdd(key, JsonSerializer.Serialize(value));
            SyncSettingsFile();
        }

        public bool TryDelete(string key)
        {
            if (!_settings.ContainsKey(key))
            {
                return false;
            }
            _settings.Remove(key);
            SyncSettingsFile();
            return true;
        }

        public bool TryRead<TValue>(string key, out TValue value)
        {
            if (!_settings.TryGetValue(key, out string serializedValue))
            {
                value = default;
                return false;
            };

            value = JsonSerializer.Deserialize<TValue>(serializedValue);
            return true;
        }

        private void SyncSettingsFile()
        {
            Exception syncException = default;
            ManualResetEvent writeCompleteEvent = new ManualResetEvent(false);
            _fileAccessSemaphore.Wait();

            Task.Run(async () =>
            {
                try
                {
                    await _fileStorage.CreateFileAsync(SettingsFileName, _settings);
                    writeCompleteEvent.Set();
                }
                catch (Exception innerException)
                {
                    syncException = innerException;
                }
            });

            writeCompleteEvent.WaitOne();
            _fileAccessSemaphore.Release();
            if (syncException != default)
            {
                throw syncException;
            }
        }
    }
}
