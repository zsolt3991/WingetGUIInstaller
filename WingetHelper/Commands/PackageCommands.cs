using System.Collections.Generic;
using WingetHelper.Decoders;
using WingetHelper.Enums;
using WingetHelper.Models;
using WingetHelper.Utils;

namespace WingetHelper.Commands
{
    public static class PackageCommands
    {
        public static WingetCommandMetadata<IEnumerable<WingetPackageEntry>> GetInstalledPackages(string filterQuery = default,
            IDictionary<PackageFilterCriteria, string> packageFilterCriteria = default, bool exactMatch = false)
        {
            var listCommand = new WingetCommandMetadata<IEnumerable<WingetPackageEntry>>("list")
                .ConfigureResultDecoder(commandResult => TabularDataDecoder.ParseResultsTable<WingetPackageEntry>(commandResult));

            if (!string.IsNullOrWhiteSpace(filterQuery))
            {
                listCommand.AddExtraArguments("--query", filterQuery);
            }

            listCommand.AddFilteringArguments(packageFilterCriteria);

            if (exactMatch)
            {
                listCommand.AddExtraArguments("--exact");
            }

            return listCommand;
        }

        public static WingetCommandMetadata<IEnumerable<WingetPackageEntry>> GetUpgradablePackages(string filterQuery = default,
            IDictionary<PackageFilterCriteria, string> packageFilterCriteria = default, bool exactMatch = false)
        {
            var upgradeCommand = new WingetCommandMetadata<IEnumerable<WingetPackageEntry>>("upgrade")
                .ConfigureResultDecoder(commandResult => TabularDataDecoder.ParseResultsTable<WingetPackageEntry>(commandResult));

            if (!string.IsNullOrWhiteSpace(filterQuery))
            {
                upgradeCommand.AddExtraArguments("--query", filterQuery);
            }

            upgradeCommand.AddFilteringArguments(packageFilterCriteria);

            if (exactMatch)
            {
                upgradeCommand.AddExtraArguments("--exact");
            }

            return upgradeCommand;
        }

        public static WingetCommandMetadata<IEnumerable<WingetPackageEntry>> SearchPackages(string searchQuery,
            IDictionary<PackageFilterCriteria, string> packageFilterCriteria = default, bool exactMatch = false)
        {
            var searchCommand = new WingetCommandMetadata<IEnumerable<WingetPackageEntry>>("search", searchQuery)
                .ConfigureResultDecoder(commandResult => TabularDataDecoder.ParseResultsTable<WingetPackageEntry>(commandResult));

            searchCommand.AddFilteringArguments(packageFilterCriteria);

            if (exactMatch)
            {
                searchCommand.AddExtraArguments("--exact");
            }

            return searchCommand;
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

        private static WingetCommandMetadata<TResult> AddFilteringArguments<TResult>(this WingetCommandMetadata<TResult> command,
            IDictionary<PackageFilterCriteria, string> packageFilterCriteria)
        {
            if (packageFilterCriteria != default)
            {
                foreach (var filterItem in packageFilterCriteria)
                {
                    command.AddExtraArguments(filterItem.Key.ToArgument(), filterItem.Value);
                }
            }
            return command;
        }
    }
}
