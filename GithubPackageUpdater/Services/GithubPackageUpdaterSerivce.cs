﻿using GithubPackageUpdater.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Octokit;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using MsixPackage = Windows.ApplicationModel.Package;

namespace GithubPackageUpdater.Services
{
    public class GithubPackageUpdaterSerivce
    {
        private readonly PackageUpdaterOptions _options;
        private readonly ILogger _logger;
        private readonly GitHubClient _client;
        private readonly PackageManager _packageManager;

        public GithubPackageUpdaterSerivce(IOptions<PackageUpdaterOptions> options, ILogger<GithubPackageUpdaterSerivce> logger = default)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? NullLogger<GithubPackageUpdaterSerivce>.Instance;
            _client = new GitHubClient(new ProductHeaderValue("msixpackageupdater"));
            _packageManager = new();

            if (!string.IsNullOrEmpty(_options.AccessToken))
            {
                _client.Credentials = new Credentials(_options.AccessToken);
            }
        }

        public async Task<PackageUpdateResponse> CheckForUpdates(MsixPackage installedPackage)
        {
            ArgumentNullException.ThrowIfNull(installedPackage);

            var packageName = installedPackage.Id.Name;
            var packageVersion = new Version(installedPackage.Id.Version.Major, installedPackage.Id.Version.Minor,
                installedPackage.Id.Version.Build, installedPackage.Id.Version.Revision);
            var packagePlatform = installedPackage.Id.Architecture.ToString();

            _logger.LogInformation("Checking for updates for: {PackageName} architecture: {PackagePlatform} version: {PackageVersion}",
                packageName, packagePlatform, packageVersion);

            var repository = await GetRepositoryAsync();
            if (repository != default)
            {
                var lastRelease = await GetLatestReleaseAsync(repository);
                if (lastRelease != default)
                {
                    if (!Version.TryParse(lastRelease.Name, out var releaseVersion))
                    {
                        _logger.LogWarning("Failed to parse github release version from release: {ReleaseName}", lastRelease.TagName);
                        if (!Version.TryParse(lastRelease.TagName, out releaseVersion))
                        {
                            _logger.LogWarning("Failed to parse github release version from tag: {TagName}", lastRelease.TagName);
                            throw new PackageUpdateException("Could not find version information in Release Name or Tag");
                        }
                    }

                    if (releaseVersion > packageVersion)
                    {
                        var packageAsset = lastRelease.Assets.FirstOrDefault(asset =>
                            asset.Name.Contains(packageName, StringComparison.InvariantCulture) &&
                            asset.Name.Contains(packagePlatform, StringComparison.InvariantCultureIgnoreCase));

                        if (packageAsset != default)
                        {
                            _logger.LogInformation("Found new version of package: {NewVersion}", releaseVersion);

                            return new PackageUpdateResponse
                            {
                                IsPackageUpToDate = false,
                                AvailableUpdateVersion = releaseVersion,
                                ChangeLog = lastRelease.Body,
                                PackageUri = new Uri(packageAsset.BrowserDownloadUrl)
                            };
                        }
                        else
                        {
                            _logger.LogWarning("Failed to find package named: {ReleaseName} architecture: {ReleasePlatform} in the github release",
                                packageName, packagePlatform);
                            throw new PackageUpdateException("Could not find package matching the required identifier in the latest release");
                        }
                    }
                    else
                    {
                        return new PackageUpdateResponse
                        {
                            IsPackageUpToDate = true,
                            PackageUri = default
                        };
                    }
                }
                else
                {
                    _logger.LogError("No releases found in the repository {RepositoryName}", repository.Name);
                    throw new PackageUpdateException("Repository has no releases published");
                }
            }
            else
            {
                _logger.LogError("Username or Repository invalid");
                throw new PackageUpdateException("Invalid Account or Repository specified");
            }

        }

        public async Task TriggerUpdate(Uri updateUrl)
        {
            ArgumentNullException.ThrowIfNull(updateUrl);

            _logger.LogInformation("Installing package from: {Url}", updateUrl);
            await _packageManager.UpdatePackageAsync(updateUrl, null, DeploymentOptions.ForceApplicationShutdown);
        }

        private async Task<Repository> GetRepositoryAsync() => await _client.Repository.Get(_options.AccountName, _options.RepositoryName);

        private async Task<Release> GetLatestReleaseAsync(Repository repository) => await _client.Repository.Release.GetLatest(repository.Id);
    }
}
