using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingetHelper.Commands;
using WingetHelper.Models;

namespace WingetGUIInstaller.Services
{
    public class PackageSourceCache
    {
        // Define a Treshold after which data is automatically fetched again
        protected static readonly TimeSpan CacheValidityTreshold = TimeSpan.FromMinutes(5);
        private readonly ConsoleOutputCache _consoleBuffer;
        private readonly ILogger<PackageSourceCache> _logger;
        private readonly ILogger<WingetCommand<object>> _commandLogger;
        private List<WingetPackageSource> _availablePackageSources;
        private DateTimeOffset _lastAvailablePackageSourcesRefresh;

        public PackageSourceCache(ConsoleOutputCache consoleOutputCache, ILogger<PackageSourceCache> logger,
            ILogger<WingetCommand<object>> commandLogger)
        {
            _consoleBuffer = consoleOutputCache;
            _logger = logger;
            _commandLogger = commandLogger;
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
            _availablePackageSources = (await PackageSourceCommands.GetPackageSources()
                .ConfigureOutputListener(_consoleBuffer.IngestMessage)
                .ConfigureLogger(_commandLogger)
                .ExecuteAsync()).ToList();
            _lastAvailablePackageSourcesRefresh = DateTimeOffset.UtcNow;
            _logger.LogInformation("Package source cache refreshed at: {time}", _lastAvailablePackageSourcesRefresh);
        }
    }
}
