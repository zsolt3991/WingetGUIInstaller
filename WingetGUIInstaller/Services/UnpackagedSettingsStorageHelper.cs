#if UNPACKAGED
using CommunityToolkit.Helpers;
using Microsoft.Windows.Storage;
using System;
using System.Text.Json;
using WingetGUIInstaller.Constants;

namespace WingetGUIInstaller.Services
{
    internal sealed class UnpackagedSettingsStorageHelper : ISettingsStorageHelper<string>
    {
        private readonly ApplicationDataContainer _container;

        public UnpackagedSettingsStorageHelper()
        {
            _container = ApplicationData.GetForUnpackaged(
                UnpackagedApplicationDataConstants.Publisher,
                UnpackagedApplicationDataConstants.Product).LocalSettings.CreateContainer(
                "settings",
                ApplicationDataCreateDisposition.Always);
        }

        public void Clear()
        {
            _container.Values.Clear();
        }

        public void Save<TValue>(string key, TValue value)
        {
            _container.Values[key] = JsonSerializer.Serialize(value);
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
            if (!_container.Values.TryGetValue(key, out object serializedValue))
            {
                value = default;
                return false;
            }

            if (serializedValue == default)
            {
                value = default;
                return false;
            }

            value = JsonSerializer.Deserialize<TValue>(serializedValue.ToString()!);
            return true;
        }
    }
}
#endif