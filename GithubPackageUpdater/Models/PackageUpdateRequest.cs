using GithubPackageUpdater.Enums;
using System;

namespace GithubPackageUpdater.Models
{
    public class PackageUpdateRequest
    {
        public string PackageName { get; set; }
        public Version PackageVersion { get; set; }
        public ProcessorArchitecture PackageArchitecture { get; set; }
        public UpdatePackageType PackageType { get; set; }
    }
}
