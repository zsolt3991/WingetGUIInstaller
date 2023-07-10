using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using WingetHelper.Commands;
using WingetHelper.Services;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class ApplicationInfoViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ICommandExecutor _commandExecutor;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WingetInstalled))]
        private Version _installedWingetVersion;

        public ApplicationInfoViewModel(DispatcherQueue dispatcherQueue, ICommandExecutor commandExecutor)
        {
#if UNPACKAGED
            var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            var resourceMap = new ResourceManager(ResourceLoader.GetDefaultResourceFilePath()).MainResourceMap;

            ApplicationName = executingAssembly.Name;
            ApplicationVersion = executingAssembly.Version;
            ApplicationPlatform = executingAssembly.ProcessorArchitecture.ToString();
            ApplicationIcon = new BitmapImage(new Uri(resourceMap.GetValue("Files/Assets/Square150x150Logo.png").ValueAsString));
#else
            ApplicationName = Package.Current.DisplayName;
            ApplicationVersion = Package.Current.Id.Version.ToVersion();
            ApplicationPlatform = Package.Current.Id.Architecture.ToString();
            ApplicationIcon = new BitmapImage(Package.Current.Logo);
#endif
            ApplicationRepositoryUrl = new Uri("https://github.com/zsolt3991/WingetGUIInstaller");
            ApplicationReportUrl = new Uri("https://github.com/zsolt3991/WingetGUIInstaller/issues/new/choose");

            _dispatcherQueue = dispatcherQueue;
            _commandExecutor = commandExecutor;
            _ = CheckWingetVersion();
        }

        public string ApplicationName { get; }

        public Version ApplicationVersion { get; }

        public string ApplicationPlatform { get; }

        public ImageSource ApplicationIcon { get; }

        public Uri ApplicationRepositoryUrl { get; }

        public Uri ApplicationReportUrl { get; }

        public bool WingetInstalled => InstalledWingetVersion != default;

        private async Task CheckWingetVersion()
        {
            var installedVersion = await _commandExecutor.ExecuteCommandAsync(GeneralCommands.GetWingetVersion());
            _dispatcherQueue.TryEnqueue(() => InstalledWingetVersion = installedVersion);
        }
    }

#if !UNPACKAGED
    internal static class VersionExtensions
    {
        public static Version ToVersion(this PackageVersion packageVersion)
        {
            return new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
    }
#endif
}
