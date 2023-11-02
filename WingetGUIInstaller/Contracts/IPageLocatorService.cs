using System;

namespace WingetGUIInstaller.Contracts
{
    public interface IPageLocatorService<in TNavigationKey> where TNavigationKey : Enum
    {
        Type GetPageTypeForKey(TNavigationKey key);
    }
}