using CommunityToolkit.Helpers;
using Microsoft.Windows.Storage;
using System.Text.Json;

namespace WingetGUIInstaller.Services
{
    internal sealed class AppDataSettingsStorageHelper : ISettingsStorageHelper<string>
    {
        private readonly ApplicationDataContainer _container;

        public AppDataSettingsStorageHelper(IApplicationDataProvider applicationDataProvider)
        {
            _container = applicationDataProvider.GetApplicationData().LocalSettings.CreateContainer(
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

            var serializedText = serializedValue.ToString();
            if (serializedText == default)
            {
                value = default;
                return false;
            }

            value = JsonSerializer.Deserialize<TValue>(serializedText);
            return true;
        }
    }
}
