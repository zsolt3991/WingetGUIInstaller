using Microsoft.UI.Xaml;

namespace WingetGUIInstaller.Utils
{
    internal static class ThemeUtils
    {
        public static ElementTheme ToElementTheme(this ApplicationTheme applictionTheme)
        {
            return applictionTheme switch
            {
                ApplicationTheme.Dark => ElementTheme.Dark,
                ApplicationTheme.Light => ElementTheme.Light,
                _ => ElementTheme.Default
            };
        }

        public static ApplicationTheme ToApplicationTheme(this ElementTheme elementTheme)
        {
            return elementTheme switch
            {
                ElementTheme.Dark => ApplicationTheme.Dark,
                ElementTheme.Light => ApplicationTheme.Light,
                _ => ApplicationTheme.Light
            };
        }
    }
}
