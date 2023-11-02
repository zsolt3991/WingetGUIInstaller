using CommunityToolkit.Common.Extensions;
using CommunityToolkit.Helpers;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using Windows.ApplicationModel.Resources;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Utils;

namespace WingetGUIInstaller.Services
{
    public sealed class ToastNotificationManager
    {
        private readonly ISettingsStorageHelper<string> _configurationStore;
        private readonly ILogger<ToastNotificationManager> _logger;
        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForViewIndependentUse();

        public ToastNotificationManager(ISettingsStorageHelper<string> configurationStore, ILogger<ToastNotificationManager> logger)
        {
            _configurationStore = configurationStore;
            _logger = logger;
            var notificationManager = AppNotificationManager.Default;
            notificationManager.NotificationInvoked += OnNotificationInvoked;
            notificationManager.Register();
        }

        ~ToastNotificationManager()
        {
            AppNotificationManager.Default.Unregister();
        }

        private bool NotificationsEnabled => _configurationStore.GetValueOrDefault(ConfigurationPropertyKeys.NotificationsEnabled,
            ConfigurationPropertyKeys.NotificationsEnabledDefaultValue);

        public void ShowGenericNotification(string titleText, string contentText)
        {
            if (!NotificationsEnabled)
                return;

            var notificationBuilder = new AppNotificationBuilder()
                .AddText(titleText)
                .AddText(contentText);

            _logger.LogDebug("Showing Notification: {title} Content: {content}", titleText, contentText);
            AppNotificationManager.Default.Show(notificationBuilder.BuildNotification());
        }

        public void ShowPackageOperationStatus(string packageName, InstallOperation installOperation, bool installComplete)
        {
            if (!NotificationsEnabled)
                return;

            var notificationBuilder = new AppNotificationBuilder()
              .AddText(packageName)
              .AddText(installComplete ?
                string.Format(_resourceLoader.GetString("PackageOperationSuccessfulFormat"), _resourceLoader.GetString(installOperation.GetResourceKey())) :
                string.Format(_resourceLoader.GetString("PackageOperationFailedFormat"), _resourceLoader.GetString(installOperation.GetResourceKey())));

            _logger.LogDebug("Showing Status Notification for package: {name}", packageName);
            AppNotificationManager.Default.Show(notificationBuilder.BuildNotification());
        }

        public void ShowBatchPackageOperationStatus(InstallOperation installOperation, int attemptedCount, int completedCount)
        {
            if (!NotificationsEnabled)
                return;

            var notificationBuilder = new AppNotificationBuilder()
                .AddText(string.Format(_resourceLoader.GetString("PackageOperationCompleteFormat"), _resourceLoader.GetString(installOperation.GetResourceKey())))
                .AddText(string.Format(_resourceLoader.GetString("SuccessfulPackageCountFormat"), completedCount));

            if (attemptedCount != completedCount)
            {
                notificationBuilder.AddText(string.Format(_resourceLoader.GetString("FailedPackageCountFormat"), attemptedCount - completedCount));
            }

            _logger.LogDebug("Showing Batch Status Notification");
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
                    .AddText(string.Format(_resourceLoader.GetString("FoundPackageUpdatesCountFormat"), updateCount));
            }
            else
            {
                notificationBuilder.AddText(_resourceLoader.GetString("PackagesUpToDateText"));
            }

            _logger.LogDebug("Showing Update availability Notification");
            AppNotificationManager.Default.Show(notificationBuilder.BuildNotification());
        }

        internal void HandleToastActivation(AppNotificationActivatedEventArgs notificationArgs)
        {
            _logger.LogDebug("Handling Toast Activation");
            if (notificationArgs.Arguments.TryGetValue("redirect", out var redirectLocationString)
                && Enum.TryParse<NavigationItemKey>(redirectLocationString, false, out var redirectLocation))
            {
                _logger.LogInformation("Handling Toast Activation: Redirect: {key}", redirectLocation);
                WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(redirectLocation));
            }
        }

        private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            HandleToastActivation(args);
        }
    }
}
