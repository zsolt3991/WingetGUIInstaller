using System;

namespace GithubPackageUpdater.Models
{
    public class PackageUpdateResponse
    {
        public bool IsPackageUpToDate { get; internal init; }
        public Version AvailableUpdateVersion { get; set; }
        public string ChangeLog { get; set; }
        public Uri PackageUri { get; internal init; }
    }
}
