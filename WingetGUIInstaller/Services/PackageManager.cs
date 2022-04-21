using System;
using System.Threading.Tasks;
using WingetHelper.Commands;
using WingetHelper.Models;

namespace WingetGUIInstaller.Services
{
    public class PackageManager
    {
        private readonly ConsoleOutputCache _consoleBuffer;

        public PackageManager(ConsoleOutputCache consoleBuffer)
        {
            _consoleBuffer = consoleBuffer;
        }

        public async Task<bool> InstallPacakge(string packageId, Action<WingetProcessState> progressListener = default)
        {
            var command = PackageCommands.InstallPackage(packageId)
                  .ConfigureOutputListener(_consoleBuffer.IngestMessage);

            if (progressListener != default)
            {
                command.ConfigureProgressListener(progressListener);
            }

            return await command.ExecuteAsync();
        }

        public async Task<bool> UpgradePackage(string packageId, Action<WingetProcessState> progressListener = default)
        {
            var command = PackageCommands.UpgradePackage(packageId)
                  .ConfigureOutputListener(_consoleBuffer.IngestMessage);

            if (progressListener != default)
            {
                command.ConfigureProgressListener(progressListener);
            }

            return await command.ExecuteAsync();
        }

        public async Task<bool> RemovePackage(string packageId, Action<WingetProcessState> progressListener = default)
        {
            var command = PackageCommands.UninstallPackage(packageId)
                  .ConfigureOutputListener(_consoleBuffer.IngestMessage);

            if (progressListener != default)
            {
                command.ConfigureProgressListener(progressListener);
            }

            return await command.ExecuteAsync();
        }
    }
}
