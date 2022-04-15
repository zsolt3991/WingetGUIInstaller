namespace WingetGUIInstaller.Constants
{
    public static class ConfigurationPropertyKeys
    {
        public const string ConsoleEnabled = "consoleEnabled";
        public const string NotificationsEnabled = "notificationsEnabled";
        public const string PackageSourceFilteringEnabled = "packageSourceFilteringEnabled";
        public const string DisabledPackageSources = "disabledPackageSources";

        public const bool ConsoleEnabledDefaultValue = true;
        public const bool NotificationsEnabledDefaultValue = true;
        public const bool PackageSourceFilteringEnabledDefaultValue = false;
        public const string DisabledPackageSourcesDefaultValue = "";
    }
}
