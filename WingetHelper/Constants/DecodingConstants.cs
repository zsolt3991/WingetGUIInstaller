using System.Collections.Generic;
using WingetHelper.Enums;
using WingetHelper.Models;

namespace WingetHelper.Constants
{
    internal static class DecodingConstants
    {
        public static readonly IReadOnlyDictionary<string, WingetProcessState> ProcessStatesMap = new Dictionary<string, WingetProcessState>
        {
            {"found", WingetProcessState.Found },
            {"downloading", WingetProcessState.Downloading },
            {"verifying" , WingetProcessState.Verifying},
            {"verified" , WingetProcessState.Verifying},
            {"starting package install" , WingetProcessState.Installing},
            {"successfully installed", WingetProcessState.Success },
            {"error", WingetProcessState.Error },
        };
    }
}
