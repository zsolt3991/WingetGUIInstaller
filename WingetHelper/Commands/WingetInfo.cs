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
    }
}
