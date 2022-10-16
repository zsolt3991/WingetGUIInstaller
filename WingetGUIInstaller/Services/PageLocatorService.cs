using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WingetGUIInstaller.Utils;

namespace WingetGUIInstaller.Services
{
    public sealed class PageLocatorService<TNavigationKey> where TNavigationKey : Enum
    {
        private IReadOnlyDictionary<TNavigationKey, Type> _pageMap;

        public PageLocatorService()
        {
            RegisterPagesInCurrentAssembly();
        }

        private void RegisterPagesInCurrentAssembly()
        {
            var pageType = typeof(Page);
            var currentAssembly = Assembly.GetExecutingAssembly();
            var discoveredPages = new Dictionary<TNavigationKey, Type>();

            // Get instances of types decorated with a Key Attribute
            foreach (var type in currentAssembly.DefinedTypes.Where(type => type.BaseType == pageType))
            {
                var keyAttribute = type.GetCustomAttribute<PageKeyAttribute>();
                if (keyAttribute != default)
                {
                    var pageKey = keyAttribute.NavigationItemKey;
                    discoveredPages.Add((TNavigationKey)pageKey, type);
                }
            }
            _pageMap = discoveredPages;
        }

        public Type GetPageTypeForKey(TNavigationKey key)
        {
            if (!_pageMap.ContainsKey(key))
            {
                throw new Exception("No Page registered for the given key");
            }

            return _pageMap[key];
        }
    }
}
