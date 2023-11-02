using Microsoft.UI.Xaml.Controls;
using System;

namespace WingetGUIInstaller.Contracts
{
    public interface IMultiLevelNavigationService<in TNavigationKey> : INavigationService<TNavigationKey> where TNavigationKey : Enum
    {
        public void AddNavigationLevel(Frame containerFrame);
        public void RemoveNavigationLevel(Frame containerFrame);
        public void ClearNavigationStack();
    }
}
