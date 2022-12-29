using Microsoft.UI.Xaml;
using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Constants
{
    public static class ConfigurationPropertyKeys
    {
        public const string AdvancedFunctionalityEnabled = "advancedFunctionalityEnabled";
        public const string NotificationsEnabled = "notificationsEnabled";
        public const string PackageSourceFilteringEnabled = "packageSourceFilteringEnabled";
        public const string DisabledPackageSources = "disabledPackageSources";
        public const string IgnoreEmptyPackageSources = "ignoreEmptyPackageSources";
        public const string AutomaticUpdates = "AutomaticUpdates";
        public const string SelectedPage = "SelectedPage";
        public const string SelectedTheme = "SelectedTheme";
        public const string ExcludedPackagesEnabled = "ExcludedPackagesEnabled";
        public const string ExcludedPackageIds = "ExcludedPackageIds";
        public const string LogLevel = "LogLevel";

        public const bool AdvancedFunctionalityEnabledDefaultValue = false;
        public const bool NotificationsEnabledDefaultValue = true;
        public const bool PackageSourceFilteringEnabledDefaultValue = false;
        public const string DisabledPackageSourcesDefaultValue = "";
        public const bool IgnoreEmptyPackageSourcesDefaultValue = true;
        public const bool AutomaticUpdatesDefaultValue = false;
        public const int SelectedPageDefaultValue = (int)NavigationItemKey.Recommendations;
        public const int SelectedThemeDefaultValue = (int)ElementTheme.Default;
        public const bool ExcludedPackagesEnabledDefaultValue = false;
        public const string ExcludedPackageIdsDefaultValue = "";
        public const int DefaultLogLevel = 2;
    }
}
