using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using CommunityToolkit.WinUI.Notifications;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Models;

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

        public void ShowPackageOperationStatus(string packageName,
            InstallOperation installOperation = InstallOperation.Install, bool installComplete = true)
        {
            if (!NotificationsEnabled)
                return;

            new ToastContentBuilder()
                .AddText(packageName)
                .AddText(installComplete ?
                    string.Format("Package {0} successful", installOperation.ToString()) :
                    string.Format("Package {0} failed", installOperation.ToString()))
                .Show();
        }

        public void ShowMultiplePackageOperationStatus(InstallOperation installOperation, int successfulCount, int failedCount)
        {
            if (!NotificationsEnabled)
                return;

            new ToastContentBuilder()
                .AddText(string.Format("Package {0} complete", installOperation.ToString()))
                .AddText(string.Format("Succesful: {0} packages", successfulCount))
                .AddText(string.Format("Failed: {0} packages ", failedCount))
                .Show();
        }

        public void ShowUpdateStatus(bool updatesAvailable, int updateCount)
        {
            if (!NotificationsEnabled)
                return;

            new ToastContentBuilder()
                .AddArgument("redirect", NavigationItemKey.Upgrades)
                .AddText(updatesAvailable ?
                    string.Format("Found {0} packages that can be upgraded", updateCount) :
                    "All packages are up to date")
                .Show();
        }

        internal void HandleToastActivation(ToastNotificationActivatedEventArgsCompat e)
        {
            var arguments = ToastArguments.Parse(e.Argument);
            var redirectLocation = arguments.GetEnum<NavigationItemKey>("redirect");
            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(redirectLocation));
        }
    }
}
