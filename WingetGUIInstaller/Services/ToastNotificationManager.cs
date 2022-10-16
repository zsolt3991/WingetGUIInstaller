using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using CommunityToolkit.WinUI.Notifications;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;

namespace WingetGUIInstaller.Services
{
    public sealed class ToastNotificationManager
    {
        private readonly ApplicationDataStorageHelper _configurationStore;

        public ToastNotificationManager(ApplicationDataStorageHelper configurationStore)
        {
            _configurationStore = configurationStore;
        }

        private bool NotificationsEnabled => _configurationStore.Read(ConfigurationPropertyKeys.NotificationsEnabled,
            ConfigurationPropertyKeys.NotificationsEnabledDefaultValue);

        public void ShowGenericNotification(string titleText, string contentText)
        {
            if (!NotificationsEnabled)
                return;

            new ToastContentBuilder()
                .AddText(titleText)
                .AddText(contentText)
                .Show();
        }

        public void ShowPackageOperationStatus(string packageName, InstallOperation installOperation, bool installComplete)
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

        public void ShowBatchPackageOperationStatus(InstallOperation installOperation, int attemptedCount, int completedCount)
        {
            if (!NotificationsEnabled)
                return;

            var toastContent = new ToastContentBuilder()
                .AddText(string.Format("Package {0} complete", installOperation.ToString()))
                .AddText(string.Format("Succesful: {0} packages", completedCount));

            if (attemptedCount != completedCount)
            {
                toastContent.AddText(string.Format("Failed: {0} packages", attemptedCount - completedCount));
            }

            toastContent.Show();
        }

        public void ShowUpdateStatus(int updateCount)
        {
            if (!NotificationsEnabled)
                return;

            var toastContent = new ToastContentBuilder();
            if (updateCount > 0)
            {
                toastContent
                    .AddArgument("redirect", NavigationItemKey.Upgrades)
                    .AddText(string.Format("Found {0} packages that can be upgraded", updateCount));
            }
            else
            {
                toastContent.AddText("All packages are up to date");
            }

            toastContent.Show();
        }

        internal void HandleToastActivation(ToastNotificationActivatedEventArgsCompat e)
        {
            var arguments = ToastArguments.Parse(e.Argument);
            var redirectLocation = arguments.GetEnum<NavigationItemKey>("redirect");
            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(redirectLocation));
        }
    }
}
