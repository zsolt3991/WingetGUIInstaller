﻿using System.Collections.Generic;
using WingetHelper.Models;
using WingetHelper.Utils;

namespace WingetHelper.Commands
{
    public static class PackageSourceCommands
    {
        public static WingetCommand<object> AddPackageSource(string name, string argument, string sourceType)
        {
            var command = new WingetCommand<object>("source", "add", "-n", name, "-a", argument)
                .ConfigureResultDecoder(commandResult => commandResult)
                .UseShellExecute()
                .AsAdministrator();

            if (!string.IsNullOrEmpty(sourceType))
            {
                command.AddExtraArguments("-t", sourceType);
            }
            return command;
        }

        public static WingetCommand<object> RemovePackageSource(string name)
        {
            return new WingetCommand<object>("source", "remove", "-n", name)
                .ConfigureResultDecoder(commandResult => commandResult)
                .UseShellExecute()
                .AsAdministrator();
        }

        public static WingetCommand<IEnumerable<WingetPackageSource>> GetPackageSources()
        {
            return new WingetCommand<IEnumerable<WingetPackageSource>>("source", "list")
                .ConfigureResultDecoder(commandResult => TabularDataDecoder.ParseResultsTable<WingetPackageSource>(commandResult));
        }
    }
}
