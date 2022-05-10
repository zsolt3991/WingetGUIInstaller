using CommunityToolkit.Mvvm.ComponentModel;
using System;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class PackageDetailsViewModel : ObservableObject
    {      

        private string _packageName;
        private string _packageId;
        private string _moniker;
        private string _version;
        private string _publisher;
        private Uri _publisherURL;
        private Uri _publisherSupportURL;
        private string _packageAuthor;
        private Uri _packageURL;
        private string _license;
        private Uri _licenseURL;
        private Uri _privacyURL;
        private string _copyright;
        private Uri _copyrightURL;
        private string _description;
        private string _releaseNotes;
        private Uri _releaseNotesUrl;

        public PackageDetailsViewModel(WingetPackageDetails wingetPackageDetails)
        {
            _packageName = wingetPackageDetails.Name;
            _packageId = wingetPackageDetails.Id;
            _moniker = wingetPackageDetails.Moniker;
            _version = wingetPackageDetails.Version;

            _publisher = wingetPackageDetails.Publisher;
            _publisherURL = !string.IsNullOrEmpty(wingetPackageDetails.Publisher_Url) ? new Uri(wingetPackageDetails.Publisher_Url) : default;
            _publisherSupportURL = !string.IsNullOrEmpty(wingetPackageDetails.Publisher_Support_Url) ? new Uri(wingetPackageDetails.Publisher_Support_Url) : default;
            _packageAuthor = wingetPackageDetails.Author;
            _packageURL = !string.IsNullOrEmpty(wingetPackageDetails.Homepage) ? new Uri(wingetPackageDetails.Homepage) : default;

            _copyright = wingetPackageDetails.Copyright;
            _copyrightURL = !string.IsNullOrEmpty(wingetPackageDetails.Copyright_Url) ? new Uri(wingetPackageDetails.Copyright_Url) : default;
            _license = wingetPackageDetails.License;
            _licenseURL = !string.IsNullOrEmpty(wingetPackageDetails.License_Url) ? new Uri(wingetPackageDetails.License_Url) : default;
            _privacyURL = !string.IsNullOrEmpty(wingetPackageDetails.Privacy_Url) ? new Uri(wingetPackageDetails.Privacy_Url) : default;

            _description = wingetPackageDetails.Description;        
            _releaseNotes = wingetPackageDetails.Release_Notes;
            _releaseNotesUrl = !string.IsNullOrEmpty(wingetPackageDetails.Release_Notes_Url) ? new Uri(wingetPackageDetails.Release_Notes_Url) : default;
        }


        public string PackageName
        {
            get => _packageName;
            private set => SetProperty(ref _packageName, value);
        }

        public string PackageId
        {
            get => _packageId;
            private set => SetProperty(ref _packageId, value);
        }

        public string PackageVersion
        {
            get => _version;
            private set => SetProperty(ref _version, value);
        }

        public string Moniker
        {
            get => _moniker;
            private set => SetProperty(ref _moniker, value);
        }

        public string PackageAuthor
        {
            get => _packageAuthor;
            private set => SetProperty(ref _packageAuthor, value);
        }

        public string Publisher
        {
            get => _publisher;
            private set => SetProperty(ref _publisher, value);
        }

        public Uri PublisherURL
        {
            get => _publisherURL;
            private set => SetProperty(ref _publisherURL, value);
        }

        public Uri PublisherSupportUrl
        {
            get => _publisherSupportURL;
            private set => SetProperty(ref _publisherSupportURL, value);
        }

        public Uri PackageURL
        {
            get => _packageURL;
            private set => SetProperty(ref _packageURL, value);
        }

        public string Copyright
        {
            get => _copyright;
            private set => SetProperty(ref _copyright, value);
        }

        public Uri CopyrightURL
        {
            get => _copyrightURL;
            private set => SetProperty(ref _copyrightURL, value);
        }

        public string License
        {
            get => _license;
            private set => SetProperty(ref _license, value);
        }

        public Uri LicenseUrl
        {
            get => _licenseURL;
            private set => SetProperty(ref _licenseURL, value);
        }

        public Uri PrivacyUrl
        {
            get => _privacyURL;
            private set => SetProperty(ref _privacyURL, value);
        }

        public string Description
        {
            get => _description;
            private set => SetProperty(ref _description, value);
        }

        public string ReleaseNotes
        {
            get => _releaseNotes;
            private set => SetProperty(ref _releaseNotes, value);
        }

        public Uri ReleaseNotesUrl
        {
            get => _releaseNotesUrl;
            private set => SetProperty(ref _releaseNotesUrl, value);
        }
    }
}
