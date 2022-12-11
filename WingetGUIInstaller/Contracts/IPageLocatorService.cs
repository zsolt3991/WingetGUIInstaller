using System;

namespace WingetGUIInstaller.Contracts
{
    public interface IPageLocatorService<TNavigationKey> where TNavigationKey : Enum
    {
        Type GetPageTypeForKey(TNavigationKey key);
    }
}