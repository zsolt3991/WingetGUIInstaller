using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;

#if UNPACKAGED
using Velopack;
#endif

namespace WingetGUIInstaller.Services
{
#if UNPACKAGED
    /// <summary>
    /// Provides VeloPack update checks and installation for unpackaged builds.
    /// </summary>
    public sealed class VeloPackUpdateService : IUpdateService
    {
        private readonly ILogger<VeloPackUpdateService> _logger;
        private UpdateManager _updateManager;
        private readonly object _updateManagerLock = new();

        public VeloPackUpdateService(ILogger<VeloPackUpdateService> logger = null)
        {
            _logger = logger ?? NullLogger<VeloPackUpdateService>.Instance;
            _logger.LogInformation("VeloPackUpdateService initialized");
        }

        private void EnsureUpdateManager()
        {
            if (_updateManager != null) return;

            lock (_updateManagerLock)
            {
                if (_updateManager != null) return;

                try
                {
                    var channel = Environment.Is64BitProcess ? "win-x64" : "win-x86";
                    _updateManager = new UpdateManager(
                        "https://github.com/zsolt3991/WingetGUIInstaller/releases/latest/download",
                        new UpdateOptions { ExplicitChannel = channel });
                    _logger.LogInformation("UpdateManager initialized with VeloPack feed and channel {Channel}", channel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize UpdateManager");
                }
            }
        }

        public async Task<IUpdateResponse> CheckForUpdatesAsync()
        {
            try
            {
                _logger.LogInformation("Checking for updates via VeloPack");

                EnsureUpdateManager();

                if (_updateManager == null)
                {
                    _logger.LogWarning("UpdateManager could not be initialized");
                    return new VeloPackUpdateResponse { IsUpdateAvailable = false };
                }

                var updateInfo = await _updateManager.CheckForUpdatesAsync();

                if (updateInfo == null)
                {
                    _logger.LogInformation("No update information available from server");
                    return new VeloPackUpdateResponse { IsUpdateAvailable = false };
                }

                var targetVersion = updateInfo.TargetFullRelease?.Version;
                if (targetVersion != null)
                {
                    _logger.LogInformation("Update available: Version {NewVersion}", targetVersion);

                    var changeLog = updateInfo.TargetFullRelease?.NotesMarkdown ?? 
                                   $"Update available: {targetVersion}";

                    return new VeloPackUpdateResponse(
                        updateVersion: targetVersion.Version,
                        changeLog: changeLog,
                        updateUri: null
                    )
                    {
                        IsUpdateAvailable = true
                    };
                }
                else
                {
                    _logger.LogInformation("Application is already up to date");
                    return new VeloPackUpdateResponse { IsUpdateAvailable = false };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for updates via VeloPack");
                return new VeloPackUpdateResponse { IsUpdateAvailable = false };
            }
        }

        public async Task InstallUpdateAsync(Uri updateUri)
        {
            try
            {
                _logger.LogInformation("VeloPack update installation triggered");

                EnsureUpdateManager();

                if (_updateManager == null)
                {
                    _logger.LogWarning("UpdateManager not initialized, cannot install update");
                    return;
                }

                var updateInfo = await _updateManager.CheckForUpdatesAsync();

                if (updateInfo?.TargetFullRelease == null)
                {
                    _logger.LogWarning("No update available for installation");
                    return;
                }

                _logger.LogInformation("Downloading VeloPack update {Version}", updateInfo.TargetFullRelease.Version);
                await _updateManager.DownloadUpdatesAsync(updateInfo);

                _logger.LogInformation("Applying VeloPack update {Version} and restarting", updateInfo.TargetFullRelease.Version);
                _updateManager.ApplyUpdatesAndRestart(updateInfo.TargetFullRelease, null);
                _logger.LogInformation("VeloPack update apply request submitted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing update via VeloPack");
            }
        }
    }
#endif
}

