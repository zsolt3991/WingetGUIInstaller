namespace WingetGUIInstaller.Constants
{
    public static class ConfigurationPropertyKeys
    {
        public const string ConsoleEnabled = "consoleEnabled";
        public const string NotificationsEnabled = "notificationsEnabled";
        public const string PackageSourceFilteringEnabled = "packageSourceFilteringEnabled";
        public const string DisabledPackageSources = "disabledPackageSources";
        public const string IgnoreEmptyPackageSources = "ignoreEmptyPackageSources";
        public const string AutomaticUpdates = "AutomaticUpdates";

        public const bool ConsoleEnabledDefaultValue = true;
        public const bool NotificationsEnabledDefaultValue = true;
        public const bool PackageSourceFilteringEnabledDefaultValue = false;
        public const string DisabledPackageSourcesDefaultValue = "";
        public const bool IgnoreEmptyPackageSourcesDefaultValue = true;
        public const bool AutomaticUpdatesDefaultValue = false;
    }
}
