using System;

namespace WingetGUIInstaller.Utils
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PageKeyAttribute : Attribute
    {
        public PageKeyAttribute(object navigationItemKey)
        {
            NavigationItemKey = navigationItemKey;
        }

        public object NavigationItemKey { get; }
    }
}
