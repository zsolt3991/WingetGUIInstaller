using CommunityToolkit.WinUI.Helpers;
using CommunityToolkit.WinUI.Notifications;
using System;
using WingetGUIInstaller.Constants;

namespace WingetGUIInstaller.Services
{
    public class ToastNotificationManager
    {
        private readonly ApplicationDataStorageHelper _configurationStore;

        public ToastNotificationManager(ApplicationDataStorageHelper configurationStore)
        {
            _configurationStore = configurationStore;
        }

        protected bool NotificationsEnabled => _configurationStore.Read(ConfigurationPropertyKeys.NotificationsEnabled,
            ConfigurationPropertyKeys.NotificationsEnabledDefaultValue);

        public void ShowPackageInstallStatus(string packageName, bool installComplete = true)
        {
            if (!NotificationsEnabled)
                return;

            new ToastContentBuilder()
                .AddText(packageName)
                .AddText(installComplete ?
                    "Package installed successfully" :
                    "Package installation failed")
                .Show();
        }

        public void ShowMultiplePackageInstallStatus(int successfulCount, int failedCount)
        {
            if (!NotificationsEnabled)
                return;

            new ToastContentBuilder()
                .AddText("Package installation complete")
                .AddText(string.Format("{0} packages were successfully installed", successfulCount))
                .AddText(string.Format("{0} packages failed to install", failedCount))
                .Show();
        }

        public void ShowUpdateStatus(bool updatesAvailable, int updateCount)
        {
            if (!NotificationsEnabled)
                return;

            new ToastContentBuilder()
                .AddText(updatesAvailable ?
                    string.Format("Found {0} packages that can be upgraded", updateCount) :
                    "All packages are up to date")
                .Show();
        }

        internal void HandleToastActivation(ToastNotificationActivatedEventArgsCompat e)
        {
        }
    }
}
