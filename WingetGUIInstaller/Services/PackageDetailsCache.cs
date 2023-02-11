using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using WingetHelper.Commands;
using WingetHelper.Models;
using WingetHelper.Services;

namespace WingetGUIInstaller.Services
{
    public sealed class PackageDetailsCache
    {
        private const int DetailsCacheSize = 5;
        private static readonly TimeSpan CacheValidityTreshold = TimeSpan.FromMinutes(5);
        private readonly ConcurrentQueue<QueueElement> _packageDetailsCache;
        private readonly ILogger<PackageDetailsCache> _logger;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ConsoleOutputCache _consoleBuffer;

        public PackageDetailsCache(ILogger<PackageDetailsCache> logger, ICommandExecutor commandExecutor,
            ConsoleOutputCache consoleBuffer)
        {
            _packageDetailsCache = new ConcurrentQueue<QueueElement>();
            _logger = logger;
            _commandExecutor = commandExecutor;
            _consoleBuffer = consoleBuffer;
        }

        public async Task<WingetPackageDetails> GetPackageDetails(string packageId, bool forceReload = false)
        {
            var cachedPackage = _packageDetailsCache.FirstOrDefault(p => p.PackageId == packageId);
            if (cachedPackage != default)
            {
                if (forceReload || DateTimeOffset.UtcNow.Subtract(CacheValidityTreshold) >= cachedPackage.LastUpdated)
                {
                    _logger.LogInformation("Package details refresh required. Force: {force}", forceReload);
                    return await LoadPackageDetailsAsync(packageId);
                }
                else
                {
                    return cachedPackage.PackageDetails;
                }
            }

            return await LoadPackageDetailsAsync(packageId);
        }

        private async Task<WingetPackageDetails> LoadPackageDetailsAsync(string packageId)
        {
            var detailsCommand = PackageCommands.GetPackageDetails(packageId)
               .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            var details = await _commandExecutor.ExecuteCommandAsync(detailsCommand);

            if (details == default)
            {
                _logger.LogWarning("No package details for packageId: {packageId}", packageId);
                return default;
            }

            if (_packageDetailsCache.Count >= DetailsCacheSize)
            {
                var cachedPackage = _packageDetailsCache.FirstOrDefault(p => p.PackageId == packageId);
                if (cachedPackage != default)
                {
                    _logger.LogInformation("Updating cached package details for packageId: {packageId}", packageId);
                    cachedPackage.PackageDetails = details;
                    cachedPackage.LastUpdated = DateTimeOffset.UtcNow;
                }
                else
                {
                    _packageDetailsCache.TryDequeue(out var itemToRemove);
                    _logger.LogInformation("Removed cached package details for packageId: {packageId}", itemToRemove?.PackageId);
                    _logger.LogInformation("Adding package details to cache for packageId: {packageId}", packageId);
                    _packageDetailsCache.Enqueue(new QueueElement
                    {
                        PackageId = packageId,
                        PackageDetails = details,
                        LastUpdated = DateTimeOffset.UtcNow
                    });
                }
            }
            else
            {
                _logger.LogInformation("Adding package details to cache for packageId: {packageId}", packageId);
                _packageDetailsCache.Enqueue(new QueueElement
                {
                    PackageId = packageId,
                    PackageDetails = details,
                    LastUpdated = DateTimeOffset.UtcNow
                });
            }

            return details;
        }

        private sealed class QueueElement
        {
            public string PackageId { get; init; }
            public DateTimeOffset LastUpdated { get; set; }
            public WingetPackageDetails PackageDetails { get; set; }
        }
    }
}
