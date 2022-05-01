using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using WingetHelper.Commands;

namespace WingetGUIInstaller.ViewModels
{
    public class ApplicationInfoViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private Version _installedWingetVersion;

        public ApplicationInfoViewModel(DispatcherQueue dispatcherQueue)
        {
            ApplicationName = Package.Current.DisplayName;
            ApplicationIconPath = "ms-appx:///Assets/Square150x150Logo.scale-200.png";
            ApplicationVersion = Package.Current.Id.Version.ToVersion();
            _dispatcherQueue = dispatcherQueue;
            _ = CheckWingetVersion();
        }

        public string ApplicationName { get; }

        public string ApplicationIconPath { get; }

        public Version ApplicationVersion { get; }

        public bool WingetInstalled => InstalledWingetVersion != default;

        public Version InstalledWingetVersion
        {
            get => _installedWingetVersion;
            private set
            {
                SetProperty(ref _installedWingetVersion, value);
                OnPropertyChanged(nameof(WingetInstalled));
            }
        }

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
