using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PackageSourceCache> _logger;
        private List<WingetPackageSource> _availablePackageSources;
        private DateTimeOffset _lastAvailablePackageSourcesRefresh;

        public PackageSourceCache(ConsoleOutputCache consoleOutputCache, ICommandExecutor commandExecutor, ILogger<PackageSourceCache> logger)
        {
            _consoleBuffer = consoleOutputCache;
            _commandExecutor = commandExecutor;
            _logger = logger;
        }

        public async Task<List<WingetPackageSource>> GetAvailablePackageSources(bool forceReload = false)
        {
            if (_availablePackageSources == default || forceReload ||
                DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= _lastAvailablePackageSourcesRefresh)
            {
                _logger.LogInformation("Package source cache refresh required. Force: {force}", forceReload);
                await LoadPackageSourceListAsync();
            }

            if (_availablePackageSources == default)
            {
                _logger.LogWarning("No package sources available");
                return new List<WingetPackageSource>();
            }

            return _availablePackageSources.ToList();
        }

        private async Task LoadPackageSourceListAsync()
        {
            var command = PackageSourceCommands.GetPackageSources()
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            var commandResult = await _commandExecutor.ExecuteCommandAsync(command);

            _availablePackageSources = commandResult != default ? commandResult.ToList() : new List<WingetPackageSource>();
            _lastAvailablePackageSourcesRefresh = DateTimeOffset.UtcNow;
            _logger.LogInformation("Package source cache refreshed at: {time} count: {count}",
                _lastAvailablePackageSourcesRefresh, _availablePackageSources.Count);
        }
    }
}
