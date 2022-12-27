using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingetHelper.Commands;
using WingetHelper.Models;
using WingetHelper.Services;

namespace WingetGUIInstaller.Services
{
    public sealed class PackageSourceCache
    {
        // Define a Treshold after which data is automatically fetched again
        private static readonly TimeSpan CacheValidityTreshold = TimeSpan.FromMinutes(5);
        private readonly ConsoleOutputCache _consoleBuffer;
        private readonly ICommandExecutor _commandExecutor;
        private List<WingetPackageSource> _availablePackageSources;
        private DateTimeOffset _lastAvailablePackageSourcesRefresh;

        public PackageSourceCache(ConsoleOutputCache consoleOutputCache, ICommandExecutor commandExecutor)
        {
            _consoleBuffer = consoleOutputCache;
            _commandExecutor = commandExecutor;
        }

        public async Task<List<WingetPackageSource>> GetAvailablePackageSources(bool forceReload = false)
        {
            if (_availablePackageSources == default || forceReload ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastAvailablePackageSourcesRefresh)
            {
                await LoadPackageSourceListAsync();
            }

            return _availablePackageSources?.ToList();
        }

        private async Task LoadPackageSourceListAsync()
        {
            var command = PackageSourceCommands.GetPackageSources()
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            var commandResult = await _commandExecutor.ExecuteCommandAsync(command);

            _availablePackageSources = commandResult != default ? commandResult.ToList() : new List<WingetPackageSource>();
            _lastAvailablePackageSourcesRefresh = DateTimeOffset.UtcNow;
        }
    }
}
