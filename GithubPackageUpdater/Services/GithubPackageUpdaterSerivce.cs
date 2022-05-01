using GithubPackageUpdater.Models;
using Octokit;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace GithubPackageUpdater.Services
{
    public class GithubPackageUpdaterSerivce
    {
        private readonly PackageUpdaterOptions _options;
        private readonly GitHubClient _client;
        private readonly PackageManager _packageManager;

        public GithubPackageUpdaterSerivce(PackageUpdaterOptions options)
        {
            _options = options;
            _client = new GitHubClient(new ProductHeaderValue("msixpackageupdater"));
            _packageManager = new();

            if (!string.IsNullOrEmpty(_options.AccessToken))
            {
                _client.Credentials = new Credentials(_options.AccessToken);
            }
        }

        public async Task<PackageUpdateResponse> CheckForUpdates(Package installedPackage)
        {
            var packageName = installedPackage.Id.Name;
            var packageVersion = new Version(installedPackage.Id.Version.Major, installedPackage.Id.Version.Minor,
                installedPackage.Id.Version.Build, installedPackage.Id.Version.Revision);

            var repository = await GetRepositoryAsync();

            if (repository != default)
            {
                var lastRelease = await GetLatestReleaseAsync(repository);
                if (lastRelease != default)
                {
                    if (!Version.TryParse(lastRelease.Name, out var releaseVersion))
                    {
                        if (!Version.TryParse(lastRelease.TagName, out releaseVersion))
                        {
                            throw new Exception("Could not find version information in Release Name or Tag");
                        }
                    }

                    if (releaseVersion >= packageVersion)
                    {
                        var packageAsset = lastRelease.Assets.FirstOrDefault(asset =>
                            asset.Name.Contains(packageName, StringComparison.InvariantCulture));
                        if (packageAsset != default)
                        {
                            return new PackageUpdateResponse
                            {
                                IsPackageUpToDate = false,
                                PackageUri = new Uri(packageAsset.BrowserDownloadUrl)
                            };
                        }
                        else
                        {
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
                    throw new Exception("Repository has no releases published");
                }
            }
            else
            {
                throw new Exception("Invalid Account or Repository specified");
            }

        }

        public async Task TriggerUpdate(Uri updateUrl)
        {
            await _packageManager.UpdatePackageAsync(updateUrl, null, DeploymentOptions.ForceApplicationShutdown);
        }

        private async Task<Repository> GetRepositoryAsync() => await _client.Repository.Get(_options.AccountName, _options.RepositoryName);

        private async Task<Release> GetLatestReleaseAsync(Repository repository) => await _client.Repository.Release.GetLatest(repository.Id);
    }
}
