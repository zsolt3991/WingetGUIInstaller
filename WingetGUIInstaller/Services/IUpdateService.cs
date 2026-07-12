using System;
using System.Threading.Tasks;

namespace WingetGUIInstaller.Services
{
    /// <summary>
    /// Abstraction for update checking and installation services.
    /// Allows different backends (GitHub for packaged, VeloPack for unpackaged, etc.)
    /// </summary>
    public interface IUpdateService
    {
        /// <summary>
        /// Checks for available updates asynchronously.
        /// </summary>
        /// <returns>Update response with version and changelog, or null if no update available</returns>
        Task<IUpdateResponse> CheckForUpdatesAsync();

        /// <summary>
        /// Triggers installation of an update from the specified URI.
        /// </summary>
        /// <param name="updateUri">URI to download and install the update from</param>
        /// <returns>Task representing the update operation</returns>
        Task InstallUpdateAsync(Uri updateUri);
    }

    /// <summary>
    /// Response from update check operation.
    /// </summary>
    public interface IUpdateResponse
    {
        /// <summary>
        /// New version available (if IsUpdateAvailable is true)
        /// </summary>
        Version UpdateVersion { get; }

        /// <summary>
        /// Changelog/release notes in markdown format
        /// </summary>
        string ChangeLog { get; }

        /// <summary>
        /// Whether an update is available
        /// </summary>
        bool IsUpdateAvailable { get; }

        /// <summary>
        /// URI to download/install the update from
        /// </summary>
        Uri UpdateUri { get; }
    }
}
