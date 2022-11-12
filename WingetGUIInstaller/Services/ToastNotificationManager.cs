using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
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

            var notificationManager = AppNotificationManager.Default;
            notificationManager.NotificationInvoked += OnNotificationInvoked;
            notificationManager.Register();
        }

        ~ToastNotificationManager()
        {
            AppNotificationManager.Default.Unregister();
        }

        private bool NotificationsEnabled => _configurationStore.Read(ConfigurationPropertyKeys.NotificationsEnabled,
            ConfigurationPropertyKeys.NotificationsEnabledDefaultValue);

        public void ShowGenericNotification(string titleText, string contentText)
        {
            if (!NotificationsEnabled)
                return;

            var notificationBuilder = new AppNotificationBuilder()
                .AddText(titleText)
                .AddText(contentText);

            AppNotificationManager.Default.Show(notificationBuilder.BuildNotification());
        }

        public void ShowPackageOperationStatus(string packageName, InstallOperation installOperation, bool installComplete)
        {
            if (!NotificationsEnabled)
                return;

            var notificationBuilder = new AppNotificationBuilder()
              .AddText(packageName)
                .AddText(installComplete ?
                    string.Format("Package {0} successful", installOperation.ToString()) :
                    string.Format("Package {0} failed", installOperation.ToString()));

            AppNotificationManager.Default.Show(notificationBuilder.BuildNotification());
        }

        public void ShowBatchPackageOperationStatus(InstallOperation installOperation, int attemptedCount, int completedCount)
        {
            if (!NotificationsEnabled)
                return;

            var notificationBuilder = new AppNotificationBuilder()
                .AddText(string.Format("Package {0} complete", installOperation.ToString()))
                .AddText(string.Format("Succesful: {0} packages", completedCount));

            if (attemptedCount != completedCount)
            {
                notificationBuilder.AddText(string.Format("Failed: {0} packages", attemptedCount - completedCount));
            }

            AppNotificationManager.Default.Show(notificationBuilder.BuildNotification());
        }

        public void ShowUpdateStatus(int updateCount)
        {
            if (!NotificationsEnabled)
                return;

            var notificationBuilder = new AppNotificationBuilder();
            if (updateCount > 0)
            {
                notificationBuilder
                    .AddArgument("redirect", NavigationItemKey.Upgrades.ToString())
                    .AddText(string.Format("Found {0} packages that can be upgraded", updateCount));
            }
            else
            {
                notificationBuilder.AddText("All packages are up to date");
            }

            AppNotificationManager.Default.Show(notificationBuilder.BuildNotification());
        }

        internal void HandleToastActivation(AppNotificationActivatedEventArgs notificationArgs)
        {
            if (notificationArgs.Arguments.TryGetValue("redirect", out var redirectLocationString))
            {
                if (Enum.TryParse<NavigationItemKey>(redirectLocationString, false, out var redirectLocation))
                {
                    WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(redirectLocation));
                }
            }
        }

        private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            HandleToastActivation(args);
        }
    }
}
