using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace WingetGUIInstaller.ViewModels
{
    public class PackageDetailsViewModel : ObservableObject
    {
        private string _packageName;
        private string _packageId;
        private string _packageAuthor;
        private Uri _packageURL;

        public string PackageName
        {
            get => _packageName;
            set => SetProperty(ref _packageName, value);
        }

        public string PackageId
        {
            get => _packageId;
            set => SetProperty(ref _packageId, value);
        }

        public string PackageAuthor
        {
            get => _packageAuthor;
            set => SetProperty(ref _packageAuthor, value);
        }

        public Uri PackageURL
        {
            get => _packageURL;
            set => SetProperty(ref _packageURL, value);
        }
    }
}
