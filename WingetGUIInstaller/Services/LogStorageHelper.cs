using System;
#if UNPACKAGED
using System.Diagnostics;
#endif
using System.IO;
using System.Threading.Tasks;
#if !UNPACKAGED
using Windows.Storage;
using Windows.System;
#endif
using WingetGUIInstaller.Constants;

namespace WingetGUIInstaller.Services
{
    internal static class LogStorageHelper
    {
        private static readonly IApplicationDataProvider _applicationDataProvider = new ApplicationDataProvider();

        public static string GetLogFileDirectory()
        {
            return Path.Combine(_applicationDataProvider.GetApplicationData().LocalPath,
                LoggingConstants.AppLogsFolderName);
        }

        public static async Task OpenLogFileDirectory()
#if UNPACKAGED
        {
            await Task.Run(() =>
            {
                Process.Start("explorer.exe", GetLogFileDirectory());
            });
        }
#else
        {
            await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(GetLogFileDirectory()));
        }
#endif
    }
}
