using System.Collections.Generic;
using WingetHelper.Models;
using WingetHelper.Utils;

namespace WingetHelper.Commands
{
    public class PackageCommands
    {
        public static WingetCommand<IEnumerable<WingetPackageEntry>> GetInstalledPackages()
        {
            return new WingetCommand<IEnumerable<WingetPackageEntry>>("list")
                .ConfigureResultDecoder(commandResult => ResponseDecoder.ParseResultsTable<WingetPackageEntry>(commandResult));
        }

        public static WingetCommand<IEnumerable<WingetPackageEntry>> GetUpgradablePackages()
        {
            return new WingetCommand<IEnumerable<WingetPackageEntry>>("upgrade")
                 .ConfigureResultDecoder(commandResult => ResponseDecoder.ParseResultsTable<WingetPackageEntry>(commandResult));
        }

        public static WingetCommand<IEnumerable<WingetPackageEntry>> SearchPackages(string searchQuery)
        {
            return new WingetCommand<IEnumerable<WingetPackageEntry>>("search", searchQuery)
                 .ConfigureResultDecoder(commandResult => ResponseDecoder.ParseResultsTable<WingetPackageEntry>(commandResult));
        }

        public static WingetCommand<WingetPackageDetails> GetPackageDetails(string packageId)
        {
            return new WingetCommand<WingetPackageDetails>("show", packageId)
                 .ConfigureResultDecoder(commandResult => ResponseDecoder.ParseDetailsResponse(commandResult));
        }

        public static WingetCommand<bool> InstallPackage(string packageId)
        {
            return new WingetCommand<bool>("install", "-e", "-h", "--id", packageId)
                .ConfigureResultDecoder(commandResult => ResponseDecoder.ParseInstallSuccessResult(commandResult));
        }

        public static WingetCommand<bool> UninstallPackage(string packageId)
        {
            return new WingetCommand<bool>("uninstall", "-e", "-h", "--id", packageId)
                .ConfigureResultDecoder(commandResult => ResponseDecoder.ParseInstallSuccessResult(commandResult));
        }

        public static WingetCommand<bool> UpgradePackage(string packageId)
        {
            return new WingetCommand<bool>("upgrade", "-e", "-h", "--id", packageId)
                .ConfigureResultDecoder(commandResult => ResponseDecoder.ParseInstallSuccessResult(commandResult));
        }
    }
}
