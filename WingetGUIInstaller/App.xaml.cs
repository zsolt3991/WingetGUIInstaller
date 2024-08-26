using CommunityToolkit.Common.Extensions;
using CommunityToolkit.Common.Helpers;
using CommunityToolkit.Helpers;
using CommunityToolkit.Mvvm.DependencyInjection;
using GithubPackageUpdater.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using Serilog;
using System.IO;
using Windows.Globalization;
using WingetGUIInstaller.Constants;
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
        private readonly ISettingsStorageHelper<string> _settingsStorage;
        private readonly IFileStorageHelper _fileStorage;
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

#if UNPACKAGED

            _fileStorage = new UnpackagedFileStorageHelper();
            _settingsStorage = new UnpackagedSettingsStorageHelper(_fileStorage);
#else

            _fileStorage = new PackagedFileStorageHelper();
            _settingsStorage = new PackagedSettingsStorageHelper();
#endif
            ConfigureServices();

            _logger = Ioc.Default.GetRequiredService<ILogger<App>>();
            _notificationManager = Ioc.Default.GetRequiredService<ToastNotificationManager>();

            var languageCode = _settingsStorage.GetValueOrDefault(ConfigurationPropertyKeys.ApplicationLanguageOverride, ConfigurationPropertyKeys.ApplicationLanguageOverrideDefaultValue);
            if (languageCode != ConfigurationPropertyKeys.ApplicationLanguageOverrideDefaultValue)
            {
                ApplicationLanguages.PrimaryLanguageOverride = languageCode;
                _logger.LogInformation("Setting application language to: {LanguageCode}", languageCode);
            }

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

                _logger.LogInformation("Launch Kind: {LaunchKind}", args.UWPLaunchActivatedEventArgs.Kind);
#if !UNPACKAGED
                _logger.LogInformation("Application Version: {Version}", Windows.ApplicationModel.Package.Current.Id.Version.ToVersion());
#else
                _logger.LogInformation("Application Version: {Version}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
#endif            
            }
        }

        private void ConfigureServices()
        {
            Ioc.Default.ConfigureServices(new ServiceCollection()
                .AddLogging(builder => builder
                    .SetMinimumLevel(GetLogLevel())
                    .AddSerilog(ConfigureLogging(), true))
                .AddSingleton(_dispatcherQueue)
                .AddSingleton(_fileStorage)
                .AddSingleton(_settingsStorage)
                .AddSingleton<ConsoleOutputCache>()
                .AddSingleton<PackageCache>()
                .AddSingleton<PackageManager>()
                .AddSingleton<PackageDetailsCache>()
                .AddSingleton<ToastNotificationManager>()
                .AddSingleton<PackageSourceCache>()
                .AddSingleton<PackageSourceManager>()
                .AddSingleton<ExclusionsManager>()
                .AddSingleton<IPageLocatorService<NavigationItemKey>, PageLocatorService<NavigationItemKey>>()
                .AddSingleton<NavigationService<NavigationItemKey>>()
                .AddSingleton<IMultiLevelNavigationService<NavigationItemKey>>(provider
                    => provider.GetRequiredService<NavigationService<NavigationItemKey>>())
                .AddSingleton<INavigationService<NavigationItemKey>>(provider
                    => provider.GetRequiredService<NavigationService<NavigationItemKey>>())
                .AddSingleton<IPackageDetailsViewModelFactory, PackageDetailsViewModelFactory>()
                .AddSingleton<HomePageViewModel>()
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
                .WriteTo.File(Path.Combine(LogStorageHelper.GetLogFileDirectory(), LoggingConstants.LogFileName),
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
                return (LogLevel)_settingsStorage.GetValueOrDefault(ConfigurationPropertyKeys.LogLevel, ConfigurationPropertyKeys.DefaultLogLevel);
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
