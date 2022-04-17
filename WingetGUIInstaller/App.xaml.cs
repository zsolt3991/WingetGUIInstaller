﻿using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Helpers;
using CommunityToolkit.WinUI.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window _window;
        private readonly ToastNotificationManager _notificationManager;
        private readonly DispatcherQueue _dipatcherQueue;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            ConfigureServices();
            InitializeComponent();
            _notificationManager = Ioc.Default.GetRequiredService<ToastNotificationManager>();
            _dipatcherQueue = Ioc.Default.GetRequiredService<DispatcherQueue>();
            Current.RequestedTheme = ApplicationTheme.Dark;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            ToastNotificationManagerCompat.OnActivated += HandleToastActivation;
            if (!ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
            {
                LaunchAndBringToForegroundIfNeeded();
            }
        }

        private void ConfigureServices()
        {
            DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            Ioc.Default.ConfigureServices(new ServiceCollection()
                .AddSingleton(dispatcherQueue)
                .AddSingleton(ApplicationDataStorageHelper.GetCurrent())
                .AddSingleton<ToastNotificationManager>()
                .AddSingleton<UpgradePageViewModel>()
                .AddSingleton<SearchPageViewModel>()
                .AddSingleton<ListPageViewModel>()
                .AddSingleton<RecommendationsPageViewModel>()
                .AddSingleton<ApplicationInfoViewModel>()
                .AddSingleton<SettingsPageViewModel>()
                .AddSingleton<ConsolePageViewModel>()
                .AddSingleton<ConsoleOutputCache>()
                .BuildServiceProvider());
        }

        private void HandleToastActivation(ToastNotificationActivatedEventArgsCompat e)
        {
            _dipatcherQueue.TryEnqueue(() =>
            {
                _notificationManager.HandleToastActivation(e);
                LaunchAndBringToForegroundIfNeeded();
            });
        }

        private void LaunchAndBringToForegroundIfNeeded()
        {
            if (_window == null)
            {
                _window = new MainWindow();
                _window.Activate();
                WindowHelper.ShowWindow(_window);
            }
            else
            {
                WindowHelper.ShowWindow(_window);
            }
        }
    }
}
