using GithubPackageUpdater.Enums;
using System;

namespace GithubPackageUpdater.Utils
{
    internal static class UpdatePackageExtensions
    {
        public static string ToFileExtension(this UpdatePackageType updatePackageType)
        {
            return updatePackageType switch
            {
                UpdatePackageType.Msi => ".msi",
                UpdatePackageType.Zip => ".zip",
                UpdatePackageType.Msix => ".msix",
                _ => throw new NotSupportedException("Unsupported Package Type " + updatePackageType)
            };
        }
    }
}
