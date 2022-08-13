using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using WingetHelper.Commands;

namespace WingetGUIInstaller.ViewModels
{
    public partial class ApplicationInfoViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WingetInstalled))]
        private Version _installedWingetVersion;

        public ApplicationInfoViewModel(DispatcherQueue dispatcherQueue)
        {
            ApplicationName = Package.Current.DisplayName;
            ApplicationVersion = Package.Current.Id.Version.ToVersion();
            ApplicationPlatform = Package.Current.Id.Architecture.ToString();
            ApplicationIcon = new BitmapImage(Package.Current.Logo);

            ApplicationRepositoryUrl = new Uri("https://github.com/zsolt3991/WingetGUIInstaller");
            ApplicationReportUrl = new Uri("https://github.com/zsolt3991/WingetGUIInstaller/issues/new/choose");

            _dispatcherQueue = dispatcherQueue;
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
            var installedVersion = await WingetInfo.GetWingetVersion().ExecuteAsync();
            _dispatcherQueue.TryEnqueue(() => InstalledWingetVersion = installedVersion);
        }
    }

    internal static class VersionExtensions
    {
        public static Version ToVersion(this PackageVersion packageVersion)
        {
            return new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
    }
}
