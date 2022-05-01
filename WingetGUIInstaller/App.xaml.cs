using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Helpers;
using GithubPackageUpdater.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            ConfigureServices();
            InitializeComponent();
            Current.RequestedTheme = ApplicationTheme.Dark;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }

        private void ConfigureServices()
        {
            DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            Ioc.Default.ConfigureServices(new ServiceCollection()
                .AddSingleton(dispatcherQueue)
                .AddSingleton(ApplicationDataStorageHelper.GetCurrent())
                .AddSingleton<ConsoleOutputCache>()
                .AddSingleton<PackageCache>()
                .AddSingleton<PackageManager>()
                .AddSingleton<MainPageViewModel>()
                .AddSingleton<UpgradePageViewModel>()
                .AddSingleton<SearchPageViewModel>()
                .AddSingleton<ListPageViewModel>()
                .AddSingleton<RecommendationsPageViewModel>()
                .AddSingleton<ApplicationInfoViewModel>()
                .AddSingleton<SettingsPageViewModel>()
                .AddSingleton<ConsolePageViewModel>()
                .AddGithubUpdater(options =>
                {
                    options
                        .ConfigureAccountName("zsolt3991")
                        .ConfigureRepository("WingetGUIInstaller");
                })
                .BuildServiceProvider());
        }
    }
}
