#if !UNPACKAGED
using CommunityToolkit.Helpers;
using System.Collections.Generic;
using System.Text.Json;
using Windows.Storage;

namespace WingetGUIInstaller.Services
{
    internal sealed class PackagedSettingsStorageHelper : ISettingsStorageHelper<string>
    {
        private readonly ApplicationDataContainer _container;

        public PackagedSettingsStorageHelper()
        {
            _container = ApplicationData.Current.LocalSettings.CreateContainer("settings", ApplicationDataCreateDisposition.Always);
        }

        public void Clear()
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(_container.Name);
        }

        public void Save<TValue>(string key, TValue value)
        {
            if (_container.Values.ContainsKey(key))
            {
                _container.Values[key] = JsonSerializer.Serialize(value);
            }
            else
            {
                _container.Values.TryAdd(key, JsonSerializer.Serialize(value));
            }
        }

        public bool TryDelete(string key)
        {
            if (!_container.Values.ContainsKey(key))
            {
                return false;
            }

            _container.Values.Remove(key);
            return true;
        }

        public bool TryRead<TValue>(string key, out TValue value)
        {
            if (!_container.Values.TryGetValue(key, out var containerValue))
            {
                value = default;
                return false;
            }

            if (containerValue == default)
            {
                value = default;
                return false;
            }

            value = JsonSerializer.Deserialize<TValue>(containerValue.ToString()!);
            return true;
        }
    }
}
#endif