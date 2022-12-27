using System.Collections.Generic;
using WingetHelper.Models;
using WingetHelper.Decoders;

namespace WingetHelper.Commands
{
    public static class PackageCommands
    {
        public static WingetCommandMetadata<IEnumerable<WingetPackageEntry>> GetInstalledPackages()
        {
            return new WingetCommandMetadata<IEnumerable<WingetPackageEntry>>("list")
                .ConfigureResultDecoder(commandResult => TabularDataDecoder.ParseResultsTable<WingetPackageEntry>(commandResult));
        }

        public static WingetCommandMetadata<IEnumerable<WingetPackageEntry>> GetUpgradablePackages()
        {
            return new WingetCommandMetadata<IEnumerable<WingetPackageEntry>>("upgrade")
                .ConfigureResultDecoder(commandResult => TabularDataDecoder.ParseResultsTable<WingetPackageEntry>(commandResult));
        }

        public static WingetCommandMetadata<IEnumerable<WingetPackageEntry>> SearchPackages(string searchQuery)
        {
            return new WingetCommandMetadata<IEnumerable<WingetPackageEntry>>("search", searchQuery)
                .ConfigureResultDecoder(commandResult => TabularDataDecoder.ParseResultsTable<WingetPackageEntry>(commandResult));
        }

        public static WingetCommandMetadata<WingetPackageDetails> GetPackageDetails(string packageId)
        {
            return new WingetCommandMetadata<WingetPackageDetails>("show", packageId)
                .ConfigureResultDecoder(commandResult => ExpressionDataDecoder.ParseDetailsResponse(commandResult));
        }

        public static WingetCommandMetadata<bool> InstallPackage(string packageId)
        {
            return new WingetCommandMetadata<bool>("install", "-e", "-h", "--id", packageId)
                .ConfigureResultDecoder(commandResult => ExpressionDataDecoder.ParseInstallSuccessResult(commandResult));
        }

        public static WingetCommandMetadata<bool> UninstallPackage(string packageId)
        {
            return new WingetCommandMetadata<bool>("uninstall", "-e", "-h", "--id", packageId)
                .ConfigureResultDecoder(commandResult => ExpressionDataDecoder.ParseUninstallSuccessResult(commandResult));
        }

        public static WingetCommandMetadata<bool> UpgradePackage(string packageId)
        {
            return new WingetCommandMetadata<bool>("upgrade", "-e", "-h", "--id", packageId)
                .ConfigureResultDecoder(commandResult => ExpressionDataDecoder.ParseInstallSuccessResult(commandResult));
        }
    }
}
