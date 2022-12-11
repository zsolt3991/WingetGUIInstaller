using Microsoft.UI.Xaml.Media.Animation;
using System;
using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Contracts
{
    public interface INavigationService<TNavigationKey> where TNavigationKey : Enum
    {
        public void GoBack();
        public void Navigate(TNavigationKey key, object args, NavigationStackMode navigationStackMode = NavigationStackMode.Add);
        public void Navigate(TNavigationKey key, NavigationTransitionInfo transitionInfo,
            object args, NavigationStackMode navigationStackMode = NavigationStackMode.Add);
    }
}
