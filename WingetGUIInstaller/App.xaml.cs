﻿using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Helpers;
using GithubPackageUpdater.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Serilog;
using System.IO;
using Windows.Storage;
using WingetGUIInstaller.Constants;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;
using WingetHelper.Extensions;

namespace WingetGUIInstaller
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private readonly ToastNotificationManager _notificationManager;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ApplicationDataStorageHelper _appDataStorage;
        private readonly ILogger<App> _logger;
        private static Window _window;

        public static Window Window => _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Current.RequestedTheme = ApplicationTheme.Dark;
            InitializeComponent();

            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _appDataStorage = ApplicationDataStorageHelper.GetCurrent();

            ConfigureServices();

            _logger = Ioc.Default.GetRequiredService<ILogger<App>>();
            _notificationManager = Ioc.Default.GetRequiredService<ToastNotificationManager>();

            UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogCritical(e.Exception, "An unhandled exception has occured");
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var currentInstance = AppInstance.GetCurrent();
            if (currentInstance.IsCurrent)
            {
                LaunchAndBringToForegroundIfNeeded();
                AppActivationArguments activationArgs = currentInstance.GetActivatedEventArgs();
                ExtendedActivationKind extendedKind = activationArgs.Kind;
                if (extendedKind == ExtendedActivationKind.AppNotification)
                {
                    var notificationActivatedEventArgs = (AppNotificationActivatedEventArgs)activationArgs.Data;
                    _notificationManager.HandleToastActivation(notificationActivatedEventArgs);
                }

                _logger.LogInformation("Launch Kind: {kind}", args.UWPLaunchActivatedEventArgs.Kind);
                _logger.LogInformation("Application Version: {version}", Windows.ApplicationModel.Package.Current.Id.Version.ToFormattedString());
            }
        }

        private void ConfigureServices()
        {
            Ioc.Default.ConfigureServices(new ServiceCollection()
                .AddLogging(builder => builder
                    .SetMinimumLevel(GetLogLevel())
                    .AddSerilog(ConfigureLogging(), true))
                .AddSingleton(_dispatcherQueue)
                .AddSingleton(_appDataStorage)
                .AddSingleton<ConsoleOutputCache>()
                .AddSingleton<PackageCache>()
                .AddSingleton<PackageManager>()
                .AddSingleton<ToastNotificationManager>()
                .AddSingleton<PackageSourceCache>()
                .AddSingleton<PackageSourceManager>()
                .AddSingleton<ExclusionsManager>()
                .AddSingleton<IPageLocatorService<NavigationItemKey>, PageLocatorService<NavigationItemKey>>()
                .AddSingleton<NavigationService<NavigationItemKey>>()
                .AddSingleton<IMultiLevelNavigationService<NavigationItemKey>>(provider => provider.GetRequiredService<NavigationService<NavigationItemKey>>())
                .AddSingleton<INavigationService<NavigationItemKey>>(provider => provider.GetRequiredService<NavigationService<NavigationItemKey>>())
                .AddSingleton<MainPageViewModel>()
                .AddSingleton<UpgradePageViewModel>()
                .AddSingleton<SearchPageViewModel>()
                .AddSingleton<ListPageViewModel>()
                .AddSingleton<RecommendationsPageViewModel>()
                .AddSingleton<ApplicationInfoViewModel>()
                .AddSingleton<SettingsPageViewModel>()
                .AddSingleton<ConsolePageViewModel>()
                .AddSingleton<ImportExportPageViewModel>()
                .AddSingleton<PackageSourcePageViewModel>()
                .AddTransient<PackageDetailsPageViewModel>()
                .AddSingleton<ExcludedPackagesViewModel>()
                .AddGithubUpdater(options => options
                    .ConfigureAccountName("zsolt3991")
                    .ConfigureRepository("WingetGUIInstaller"))
                .AddWingetHelper()
                .BuildServiceProvider());
        }

        private Serilog.ILogger ConfigureLogging()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Is(GetLogLevel().ToSerilogLevel())
                .WriteTo.File(
                    Path.Combine(ApplicationData.Current.LocalFolder.Path, LoggingConstants.LogFileName),
                    outputTemplate: LoggingConstants.LogTemplate,
                    rollingInterval: RollingInterval.Day)
#if DEBUG
                .WriteTo.Debug(outputTemplate: LoggingConstants.LogTemplate)
#endif
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        private LogLevel GetLogLevel()
        {
            try
            {
                return (LogLevel)_appDataStorage.Read(ConfigurationPropertyKeys.LogLevel, ConfigurationPropertyKeys.DefaultLogLevel);
            }
            catch
            {
                return LogLevel.Information;
            }
        }

        private static void LaunchAndBringToForegroundIfNeeded()
        {
            if (_window == null)
            {
                _window = new MainWindow();
                _window.Activate();
                WindowInteropUtils.ShowWindow(_window);
            }
            else
            {
                WindowInteropUtils.ShowWindow(_window);
            }
        }
    }
}
