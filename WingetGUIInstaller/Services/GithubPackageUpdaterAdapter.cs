using GithubPackageUpdater.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace WingetGUIInstaller.Services
{
    /// <summary>
    /// Adapter that wraps GithubPackageUpdaterService to implement IUpdateService interface.
    /// Allows GitHub updater to be used through the unified IUpdateService abstraction.
    /// Used for packaged builds (MSIX).
    /// </summary>
    public sealed class GithubPackageUpdaterAdapter : IUpdateService
    {
        private readonly GithubPackageUpdaterSerivce _githubUpdater;
        private readonly ILogger<GithubPackageUpdaterAdapter> _logger;

        public GithubPackageUpdaterAdapter(GithubPackageUpdaterSerivce githubUpdater, ILogger<GithubPackageUpdaterAdapter> logger = null)
        {
            _githubUpdater = githubUpdater ?? throw new ArgumentNullException(nameof(githubUpdater));
            _logger = logger ?? NullLogger<GithubPackageUpdaterAdapter>.Instance;
        }

        /// <summary>
        /// Checks for updates using the GitHub updater.
        /// Adapts PackageUpdateResponse to IUpdateResponse interface.
        /// </summary>
        public async Task<IUpdateResponse> CheckForUpdatesAsync()
        {
            try
            {
                _logger.LogInformation("Checking for updates via GitHub");

                var response = await _githubUpdater.CheckForUpdates(Package.Current);

                if (response == null)
                {
                    _logger.LogInformation("No update response from GitHub");
                    return new GithubUpdateResponseAdapter();
                }

                _logger.LogInformation("GitHub update check complete. Update available: {IsUpdateAvailable}", 
                    !response.IsPackageUpToDate);

                return new GithubUpdateResponseAdapter(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for updates via GitHub");
                return new GithubUpdateResponseAdapter();
            }
        }

        /// <summary>
        /// Triggers update installation via GitHub updater.
        /// </summary>
        public async Task InstallUpdateAsync(Uri updateUri)
        {
            try
            {
                _logger.LogInformation("Installing update from: {UpdateUri}", updateUri);

                if (updateUri == null)
                {
                    _logger.LogWarning("Update URI is null, cannot install");
                    return;
                }

                // GitHub updater's TriggerUpdate expects a URI
                await _githubUpdater.TriggerUpdate(updateUri);

                _logger.LogInformation("Update triggered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing update");
                throw;
            }
        }
    }

    /// <summary>
    /// Adapter that wraps PackageUpdateResponse to implement IUpdateResponse interface.
    /// Bridges the GitHub updater's response type to the unified interface.
    /// </summary>
    internal sealed class GithubUpdateResponseAdapter : IUpdateResponse
    {
        private readonly GithubPackageUpdater.Models.PackageUpdateResponse _response;

        public Version UpdateVersion => _response?.AvailableUpdateVersion;

        public string ChangeLog => _response?.ChangeLog ?? string.Empty;

        public bool IsUpdateAvailable => _response != null && !_response.IsPackageUpToDate;

        public Uri UpdateUri => _response?.PackageUri;

        /// <summary>
        /// Creates adapter from GitHub PackageUpdateResponse
        /// </summary>
        public GithubUpdateResponseAdapter(GithubPackageUpdater.Models.PackageUpdateResponse response)
        {
            _response = response;
        }

        /// <summary>
        /// Creates empty response (for "no update available" case)
        /// </summary>
        public GithubUpdateResponseAdapter() : this(null)
        {
        }
    }
}
