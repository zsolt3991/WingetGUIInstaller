using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
#if !UNPACKAGED
using Windows.Storage;
using Windows.System;
#endif
using WingetGUIInstaller.Constants;

namespace WingetGUIInstaller.Services
{
    internal sealed class LogStorageHelper
    {

        public static string GetLogFileDirectory()
#if UNPACKAGED
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                StorageFolderConstants.ApplicationFolderName, LoggingConstants.AppLogsFolderName);
        }
#else
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, LoggingConstants.AppLogsFolderName);
        }
#endif

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
