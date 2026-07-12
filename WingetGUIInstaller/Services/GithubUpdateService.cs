using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Octokit;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Models;
using MsixPackage = Windows.ApplicationModel.Package;

#if !UNPACKAGED
namespace WingetGUIInstaller.Services
{
    public sealed class GithubUpdateService : IUpdateService
    {
        private const string AccountName = "zsolt3991";
        private const string RepositoryName = "WingetGUIInstaller";

        private readonly ILogger<GithubUpdateService> _logger;
        private readonly GitHubClient _client;
        private readonly Windows.Management.Deployment.PackageManager _packageManager = new();

        public GithubUpdateService(ILogger<GithubUpdateService> logger = null)
        {
            _logger = logger ?? NullLogger<GithubUpdateService>.Instance;
            _client = new GitHubClient(new ProductHeaderValue("WingetGUIInstaller"));
        }

        public async Task<IUpdateResponse> CheckForUpdatesAsync()
        {
            try
            {
                var installedPackage = MsixPackage.Current;
                var packageName = installedPackage.Id.Name;
                var packageVersion = new Version(
                    installedPackage.Id.Version.Major,
                    installedPackage.Id.Version.Minor,
                    installedPackage.Id.Version.Build,
                    installedPackage.Id.Version.Revision);
                var packagePlatform = installedPackage.Id.Architecture.ToString();

                _logger.LogInformation(
                    "Checking for updates for {PackageName}, architecture {PackagePlatform}, version {PackageVersion}",
                    packageName,
                    packagePlatform,
                    packageVersion);

                var repository = await _client.Repository.Get(AccountName, RepositoryName);
                var release = await _client.Repository.Release.GetLatest(repository.Id);

                if (!Version.TryParse(release.Name, out var releaseVersion) &&
                    !Version.TryParse(release.TagName, out releaseVersion))
                {
                    _logger.LogWarning("Could not parse version from release {ReleaseName} or tag {TagName}", release.Name, release.TagName);
                    return new GithubUpdateResponse();
                }

                if (releaseVersion <= packageVersion)
                {
                    _logger.LogInformation("Application is already up to date");
                    return new GithubUpdateResponse();
                }

                var packageAsset = release.Assets.FirstOrDefault(asset =>
                    asset.Name.Contains(packageName, StringComparison.InvariantCulture) &&
                    asset.Name.Contains(packagePlatform, StringComparison.InvariantCultureIgnoreCase));

                if (packageAsset == null)
                {
                    _logger.LogWarning(
                        "No package asset found for {PackageName}, architecture {PackagePlatform}",
                        packageName,
                        packagePlatform);
                    return new GithubUpdateResponse();
                }

                _logger.LogInformation("Update available: {Version}", releaseVersion);
                return new GithubUpdateResponse(
                    releaseVersion,
                    release.Body,
                    new Uri(packageAsset.BrowserDownloadUrl));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for updates via GitHub");
                return new GithubUpdateResponse();
            }
        }

        public async Task InstallUpdateAsync(Uri updateUri)
        {
            if (updateUri == null)
            {
                _logger.LogWarning("Update URI is null, cannot install update");
                return;
            }

            _logger.LogInformation("Installing update from {UpdateUri}", updateUri);
            await _packageManager.UpdatePackageAsync(updateUri, null, DeploymentOptions.ForceApplicationShutdown);
        }
    }
}
#endif
