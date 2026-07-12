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
    /// VeloPack implementation of IUpdateService for unpackaged builds.
    /// Handles update checking and installation using VeloPack's UpdateManager.
    /// </summary>
    public sealed class VeloPackUpdateService : IUpdateService
    {
        private readonly ILogger<VeloPackUpdateService> _logger;
        private UpdateManager _updateManager;
        private readonly object _updateManagerLock = new object();

        public VeloPackUpdateService(ILogger<VeloPackUpdateService> logger = null)
        {
            _logger = logger ?? NullLogger<VeloPackUpdateService>.Instance;
            _logger.LogInformation("VeloPackUpdateService initialized");
        }

        /// <summary>
        /// Checks for available updates from VeloPack sources.
        /// Uses UpdateManager to query GitHub Releases or configured update source.
        /// </summary>
        public async Task<IUpdateResponse> CheckForUpdatesAsync()
        {
            try
            {
                _logger.LogInformation("Checking for updates via VeloPack");

                // Initialize UpdateManager lazily on first check (thread-safe)
                if (_updateManager == null)
                {
                    lock (_updateManagerLock)
                    {
                        if (_updateManager == null)
                        {
                            _updateManager = new UpdateManager("https://github.com/zsolt3991/WingetGUIInstaller");
                            _logger.LogInformation("UpdateManager initialized with GitHub repository");
                        }
                    }
                }

                // Check for available updates
                var updateInfo = await _updateManager.CheckForUpdatesAsync();

                if (updateInfo == null)
                {
                    _logger.LogInformation("No update information available from server");
                    return new VeloPackUpdateResponse { IsUpdateAvailable = false };
                }

                // Check if update is available
                if (updateInfo.IsUpdateAvailable)
                {
                    var newVersion = updateInfo.TargetFullRelease?.Version;
                    _logger.LogInformation("Update available: Version {NewVersion}", newVersion);

                    var changeLog = updateInfo.TargetFullRelease?.ReleaseNotes ?? 
                                   $"New version {newVersion} is available";

                    return new VeloPackUpdateResponse(
                        updateVersion: newVersion,
                        changeLog: changeLog,
                        updateUri: null // VeloPack handles URI internally
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

        /// <summary>
        /// Installs an available update.
        /// VeloPack handles downloading, staging, and applying the update.
        /// </summary>
        public async Task InstallUpdateAsync(Uri updateUri)
        {
            try
            {
                _logger.LogInformation("VeloPack update installation triggered");

                if (_updateManager == null)
                {
                    _logger.LogWarning("UpdateManager not initialized, cannot install update");
                    return;
                }

                // Check for updates again to get the latest info
                var updateInfo = await _updateManager.CheckForUpdatesAsync();

                if (updateInfo == null || !updateInfo.IsUpdateAvailable)
                {
                    _logger.LogWarning("No update available for installation");
                    return;
                }

                _logger.LogInformation("Downloading update {Version}", updateInfo.TargetFullRelease?.Version);

                // Download and apply update (this will restart the app)
                await _updateManager.ApplyUpdatesAndRestartAsync(updateInfo);

                _logger.LogInformation("Update installation completed, app will restart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing update via VeloPack");
                throw;
            }
        }

        public void Dispose()
        {
            _updateManager?.Dispose();
        }
    }
#endif
}
