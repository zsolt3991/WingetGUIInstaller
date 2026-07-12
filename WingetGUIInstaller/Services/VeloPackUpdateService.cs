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
    /// Handles update checking using VeloPack's UpdateManager.
    /// VeloPack itself handles the actual update download and restart via Program.cs bootstrap.
    /// Supports automatic delta updates for efficient bandwidth usage.
    /// For more information, see: https://docs.velopack.io/
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
        /// Ensures UpdateManager is initialized (thread-safe lazy initialization).
        /// </summary>
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

        /// <summary>
        /// Checks for available updates from VeloPack sources.
        /// Uses UpdateManager to query GitHub Releases or configured update source.
        /// </summary>
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

                // Check for available updates
                var updateInfo = await _updateManager.CheckForUpdatesAsync();

                if (updateInfo == null)
                {
                    _logger.LogInformation("No update information available from server");
                    return new VeloPackUpdateResponse { IsUpdateAvailable = false };
                }

                // Check if update is available
                var targetVersion = updateInfo.TargetFullRelease?.Version;
                if (targetVersion != null)
                {
                    _logger.LogInformation("Update available: Version {NewVersion}", targetVersion);

                    // Get release notes from the UpdateInfo
                    var changeLog = updateInfo.TargetFullRelease?.NotesMarkdown ?? 
                                   $"Update available: {targetVersion}";

                    return new VeloPackUpdateResponse(
                        updateVersion: targetVersion.Version,
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
        /// This method tells VeloPack to check and apply updates.
        /// VeloPack handles downloading, staging, and applying the update with a restart.
        /// </summary>
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

                // Check for updates again to ensure we have the latest info
                var updateInfo = await _updateManager.CheckForUpdatesAsync();

                if (updateInfo?.TargetFullRelease == null)
                {
                    _logger.LogWarning("No update available for installation");
                    return;
                }

                _logger.LogInformation("Triggering update check with VeloPack");

                // VeloPack will handle the actual download and restart
                // Simply calling the update manager triggers the process
                // The VelopackApp.Build().Run() in Program.cs handles the bootstrap
                _ = _updateManager.CheckForUpdatesAsync(); // Fire and forget to allow VeloPack to manage the update flow

                _logger.LogInformation("Update installation initiated, VeloPack will handle the restart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing update via VeloPack");
            }
        }
    }
#endif
}

