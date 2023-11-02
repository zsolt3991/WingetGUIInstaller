using System.Collections.Generic;
using WingetHelper.Decoders;
using WingetHelper.Models;

namespace WingetHelper.Commands
{
    public static class PackageSourceCommands
    {
        public static WingetCommandMetadata<object> AddPackageSource(string name, string argument, string sourceType)
        {
            var command = new WingetCommandMetadata<object>("source", "add", "-n", name, "-a", argument)
                .ConfigureResultDecoder(commandResult => commandResult)
                .RunAsAdministrator();

            if (!string.IsNullOrEmpty(sourceType))
            {
                command.AddExtraArguments("-t", sourceType);
            }
            return command;
        }

        public static WingetCommandMetadata<object> RemovePackageSource(string name)
        {
            return new WingetCommandMetadata<object>("source", "remove", "-n", name)
                .ConfigureResultDecoder(commandResult => commandResult)
                .RunAsAdministrator();
        }

        public static WingetCommandMetadata<IEnumerable<WingetPackageSource>> GetPackageSources()
        {
            return new WingetCommandMetadata<IEnumerable<WingetPackageSource>>("source", "list")
                .ConfigureResultDecoder(commandResult => TabularDataDecoder.ParseResultsTable<WingetPackageSource>(commandResult));
        }
    }
}
