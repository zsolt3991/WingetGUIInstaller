using System;
using WingetHelper.Utils;

namespace WingetHelper.Commands
{
    public static class WingetInfo
    {
        public static WingetCommand<Version> GetWingetVersion()
        {
            return new WingetCommand<Version>("--version")
                .ConfigureResultDecoder(commandResult => ResponseDecoder.ParseResultsVersion(commandResult));
        }

        public static WingetCommand<object> CustomWingetCommand(string[] arguments, bool requiresAdmin = false)
        {
            var command = new WingetCommand<object>(arguments);

            if (requiresAdmin)
            {
                command.UseShellExecute().AsAdministrator();
            }

            return command;
        }
    }
}
