using System;
using WingetHelper.Decoders;

namespace WingetHelper.Commands
{
    public static class GeneralCommands
    {
        public static WingetCommandMetadata<Version> GetWingetVersion()
        {
            return new WingetCommandMetadata<Version>("--version")
                .ConfigureResultDecoder(commandResult => ExpressionDataDecoder.ParseResultsVersion(commandResult));
        }

        public static WingetCommandMetadata<object> CustomWingetCommand(string[] arguments, bool requiresAdmin = false)
        {
            var command = new WingetCommandMetadata<object>(arguments);

            if (requiresAdmin)
            {
                command.RunAsAdministrator();
            }

            return command;
        }
    }
}
