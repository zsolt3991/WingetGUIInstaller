using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Windows.ApplicationModel;
using WingetHelper.Commands;

namespace WingetGUIInstaller.ViewModels
{
    public class ApplicationInfoViewModel : ObservableObject
    {
        public ApplicationInfoViewModel()
        {
            ApplicationName = Package.Current.DisplayName;
            ApplicationIconPath = "ms-appx:///Assets/Square150x150Logo.scale-200.png";
            ApplicationVersion = Package.Current.Id.Version.ToVersion();
            CheckWingetVersion();
        }

        private async void CheckWingetVersion()
        {

            InstalledWingetVersion = await WingetInfo.GetWingetVersion()
                .ExecuteAsync();
            OnPropertyChanged(nameof(InstalledWingetVersion));
            OnPropertyChanged(nameof(WingetInstalled));
        }

        public string ApplicationName { get; }
        public string ApplicationIconPath { get; }
        public Version ApplicationVersion { get; }
        public Version InstalledWingetVersion { get; private set; }
        public bool WingetInstalled => InstalledWingetVersion != default;
    }

    internal static class VersionExtensions
    {
        public static Version ToVersion(this PackageVersion packageVersion)
        {
            return new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Revision, packageVersion.Build);
        }
    }
}
