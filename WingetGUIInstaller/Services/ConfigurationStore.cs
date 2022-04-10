using System;
using System.Collections.Generic;
using Windows.Storage;

namespace WingetGUIInstaller.Services
{
    public class ConfigurationStore
    {
        private readonly ApplicationDataContainer _settingsContainer;

        public ConfigurationStore()
        {
            if (!ApplicationData.Current.LocalSettings.Containers.TryGetValue("settings", out _settingsContainer))
            {
                _settingsContainer = ApplicationData.Current.LocalSettings.CreateContainer("settings", ApplicationDataCreateDisposition.Always);
            }
        }

        public TValue GetStoredProperty<TValue>(string propertyKey, TValue defaultValue = default) 
        {
            if (_settingsContainer.Values.ContainsKey(propertyKey))
            {
                return (TValue)Convert.ChangeType(_settingsContainer.Values[propertyKey], typeof(TValue));
            }
            else
            {
                StoreProperty(propertyKey, defaultValue);
                return defaultValue;
            }
        }

        public bool StoreProperty<TValue>(string propertyKey, TValue propertyValue)
        {
            if (!_settingsContainer.Values.ContainsKey(propertyKey))
            {
                return _settingsContainer.Values.TryAdd(propertyKey, propertyValue);
            }
            else
            {
                _settingsContainer.Values[propertyKey] = propertyValue;
                return true;
            }
        }
    }
}
