using System;

namespace GithubPackageUpdater.Models
{
    public class PackageUpdateResponse
    {
        public bool IsPackageUpToDate { get; internal init; }
        public Version AvailableUpdateVersion { get; internal init; }
        public string ChangeLog { get; internal init; }
        public Uri PackageUri { get; internal init; }
    }
}
