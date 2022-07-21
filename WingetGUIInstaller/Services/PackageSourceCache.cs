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
        private ConsoleOutputCache _consoleBuffer;
        private List<WingetPackageSource> _availablePackageSources;
        private DateTimeOffset _lastAvailablePackageSourcesRefresh;

        public PackageSourceCache(ConsoleOutputCache consoleOutputCache)
        {
            _consoleBuffer = consoleOutputCache;
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
            var commandResult = await PackageSourceCommands.GetPackageSources()
                .ConfigureOutputListener(_consoleBuffer.IngestMessage)
                .ExecuteAsync();

            _availablePackageSources = commandResult != default ? commandResult.ToList() : new List<WingetPackageSource>();
            _lastAvailablePackageSourcesRefresh = DateTimeOffset.UtcNow;
        }
    }
}
