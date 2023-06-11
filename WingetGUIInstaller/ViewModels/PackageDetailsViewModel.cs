using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class PackageDetailsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _packageName;

        [ObservableProperty]
        private string _packageId;

        [ObservableProperty]
        private string _moniker;

        [ObservableProperty]
        private string _version;

        [ObservableProperty]
        private string _publisher;

        [ObservableProperty]
        private Uri _publisherURL;

        [ObservableProperty]
        private Uri _publisherSupportURL;

        [ObservableProperty]
        private string _packageAuthor;

        [ObservableProperty]
        private Uri _packageURL;

        [ObservableProperty]
        private string _license;

        [ObservableProperty]
        private Uri _licenseURL;

        [ObservableProperty]
        private Uri _privacyURL;

        [ObservableProperty]
        private string _copyright;

        [ObservableProperty]
        private Uri _copyrightURL;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private string _releaseNotes;

        [ObservableProperty]
        private Uri _releaseNotesUrl;

        [ObservableProperty]
        private ObservableCollection<PackageTagViewModel> _tags;

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

            _tags = new ObservableCollection<PackageTagViewModel>(
                wingetPackageDetails.Tags.Select(tag => new PackageTagViewModel(tag)));
        }
    }
}
