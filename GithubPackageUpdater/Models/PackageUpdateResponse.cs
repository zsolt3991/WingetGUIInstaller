using System;

namespace GithubPackageUpdater.Models
{
    public class PackageUpdateResponse
    {
        public bool IsPackageUpToDate { get; internal init; }
        public Uri PackageUri { get; internal init; }
    }
}
