using System;

namespace GithubPackageUpdater.Models
{
    public class PackageUpdateResponse
    {
        public required bool IsPackageUpToDate { get; init; }
        public Version? AvailableUpdateVersion { get; init; }
        public string? ChangeLog { get; init; }
        public Uri? PackageUri { get; init; }
    }
}
