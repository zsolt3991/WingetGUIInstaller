using Microsoft.UI.Xaml.Media.Animation;
using System;
using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Contracts
{
    public interface INavigationService<TNavigationKey> where TNavigationKey : Enum
    {
        public void GoBack();
        void GoForward();
        public void Navigate(TNavigationKey key, NavigationTransitionInfo transitionInfo = default,
            object args = default, NavigationStackMode navigationStackMode = NavigationStackMode.Add);
    }
}
