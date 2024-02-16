using GithubPackageUpdater.Enums;
using System;

namespace GithubPackageUpdater.Models
{
    public class PackageUpdateRequest
    {
        public required string PackageName { get; init; }
        public required Version PackageVersion { get; init; }
        public required ProcessorArchitecture PackageArchitecture { get; init; }
        public required UpdatePackageType PackageType { get; init; }
    }
}
