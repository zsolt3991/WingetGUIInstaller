using GithubPackageUpdater.Models;
using GithubPackageUpdater.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Octokit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GithubPackageUpdater.Services
{
    public class GithubPackageUpdaterSerivce
    {
        private readonly PackageUpdaterOptions _options;
        private readonly ILogger _logger;
        private readonly GitHubClient _client;

        public GithubPackageUpdaterSerivce(IOptions<PackageUpdaterOptions> options, ILogger<GithubPackageUpdaterSerivce> logger = default)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? NullLogger<GithubPackageUpdaterSerivce>.Instance;
            _client = new GitHubClient(new ProductHeaderValue("githubpackageupdater"));

            if (!string.IsNullOrEmpty(_options.AccessToken))
            {
                _client.Credentials = new Credentials(_options.AccessToken);
            }
        }

        public async Task<PackageUpdateResponse> CheckForUpdates(PackageUpdateRequest packageUpdateRequest)
        {
            if (packageUpdateRequest == default)
            {
                throw new ArgumentNullException(nameof(packageUpdateRequest));
            }

            _logger.LogInformation("Checking for updates for: {packageName} architecture: {packagePlatform} version: {packageVersion}",
                packageUpdateRequest.PackageName, packageUpdateRequest.PackageArchitecture, packageUpdateRequest.PackageVersion);

            var repository = await GetRepositoryAsync();
            if (repository != default)
            {
                var lastRelease = await GetLatestReleaseAsync(repository);
                if (lastRelease != default)
                {
                    if (!Version.TryParse(lastRelease.Name, out var releaseVersion))
                    {
                        _logger.LogWarning("Failed to parse github release version from release: {releaseName}", lastRelease.TagName);
                        if (!Version.TryParse(lastRelease.TagName, out releaseVersion))
                        {
                            _logger.LogWarning("Failed to parse github release version from tag: {tagName}", lastRelease.TagName);
                            throw new Exception("Could not find version information in Release Name or Tag");
                        }
                    }

                    if (releaseVersion > packageUpdateRequest.PackageVersion)
                    {
                        var packageAsset = lastRelease.Assets.FirstOrDefault(asset =>
                            asset.Name.StartsWith(packageUpdateRequest.PackageName, StringComparison.InvariantCulture) &&
                            asset.Name.Contains(packageUpdateRequest.PackageArchitecture.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                            asset.Name.EndsWith(packageUpdateRequest.PackageType.ToFileExtension(), StringComparison.OrdinalIgnoreCase));

                        if (packageAsset != default)
                        {
                            _logger.LogInformation("Found new version of package: {newVersion}", releaseVersion);

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
                            _logger.LogWarning("Failed to find package named: {releaseName} architecture: {releasePlatform} in the github release",
                                packageUpdateRequest.PackageName, packageUpdateRequest.PackageArchitecture);
                            throw new Exception("Could not find package matching the required identifier in the latest release");
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
                    _logger.LogError("No releases found in the repository {repositoryName}", repository.Name);
                    throw new Exception("Repository has no releases published");
                }
            }
            else
            {
                _logger.LogError("Username or Repository invalid");
                throw new Exception("Invalid Account or Repository specified");
            }
        }

        private async Task<Repository> GetRepositoryAsync() => await _client.Repository.Get(_options.AccountName, _options.RepositoryName);

        private async Task<Release> GetLatestReleaseAsync(Repository repository) => await _client.Repository.Release.GetLatest(repository.Id);
    }
}
