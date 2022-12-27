using System;
using System.Threading.Tasks;
using Windows.Storage;
using WingetHelper.Commands;
using WingetHelper.Models;
using WingetHelper.Services;

namespace WingetGUIInstaller.Services
{
    public sealed class PackageManager
    {
        private readonly ConsoleOutputCache _consoleBuffer;
        private readonly PackageCache _packageCache;
        private readonly ICommandExecutor _commandExecutor;

        public PackageManager(ConsoleOutputCache consoleBuffer, PackageCache packageCache, ICommandExecutor commandExecutor)
        {
            _consoleBuffer = consoleBuffer;
            _packageCache = packageCache;
            _commandExecutor = commandExecutor;
        }

        public async Task<bool> InstallPacakge(string packageId, Action<WingetProcessState> progressListener = default)
        {
            var command = PackageCommands.InstallPackage(packageId)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);

            if (progressListener != default)
            {
                command.ConfigureProgressListener(progressListener);
            }

            var result = await _commandExecutor.ExecuteCommandAsync(command);
            if (result)
            {
                _packageCache.InvalidateCache();
            }
            return result;
        }

        public async Task<bool> UpgradePackage(string packageId, Action<WingetProcessState> progressListener = default)
        {
            var command = PackageCommands.UpgradePackage(packageId)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);

            if (progressListener != default)
            {
                command.ConfigureProgressListener(progressListener);
            }

            var result = await _commandExecutor.ExecuteCommandAsync(command);
            if (result)
            {
                _packageCache.InvalidateCache();
            }
            return result;
        }

        public async Task<bool> RemovePackage(string packageId, Action<WingetProcessState> progressListener = default)
        {
            var command = PackageCommands.UninstallPackage(packageId)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);

            if (progressListener != default)
            {
                command.ConfigureProgressListener(progressListener);
            }

            var result = await _commandExecutor.ExecuteCommandAsync(command);
            if (result)
            {
                _packageCache.InvalidateCache();
            }
            return result;
        }

        public async Task ExportPackageList(StorageFile outputFile, bool exportVersions, string packageSourceFilter)
        {
            var sourceToExport = !string.IsNullOrEmpty(packageSourceFilter) ? packageSourceFilter : null;
            var command = PackageListCommands.ExportPackagesToFile(outputFile.Path, exportVersions, false, sourceToExport)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            await _commandExecutor.ExecuteCommandAsync(command);

        }

        public async Task ImportPackageList(StorageFile inputFile, bool importVersions, bool ignoreMissing)
        {
            var command = PackageListCommands.ImportPackagesFromFile(inputFile.Path, ignoreMissing, importVersions)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            await _commandExecutor.ExecuteCommandAsync(command);
        }
    }
}
