using System;

namespace WingetGUIInstaller.Utils
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NavigationKeyAttribute : Attribute
    {
        public NavigationKeyAttribute(object navigationItemKey)
        {
            NavigationItemKey = navigationItemKey;
        }

        public object NavigationItemKey { get; }
    }
}
