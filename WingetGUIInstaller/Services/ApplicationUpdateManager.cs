using CommunityToolkit.Common.Helpers;
using GithubPackageUpdater.Models;
using GithubPackageUpdater.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
#if !UNPACKAGED
using Windows.ApplicationModel;
#endif
using WingetGUIInstaller.Utils;

namespace WingetGUIInstaller.Services
{
    public sealed class ApplicationUpdateManager
    {
#if UNPACKAGED
        private const string ApplicationPackageName = "WingetGUIInstaller";
# endif
        private readonly ILogger<ApplicationUpdateManager> _logger;
        private readonly GithubPackageUpdaterSerivce _githubPackageUpdaterSerivce;
        private readonly IFileStorageHelper _fileStorageHelper;

        public ApplicationUpdateManager(ILogger<ApplicationUpdateManager> logger, GithubPackageUpdaterSerivce githubPackageUpdaterSerivce,
            IFileStorageHelper fileStorageHelper)
        {
            _logger = logger;
            _githubPackageUpdaterSerivce = githubPackageUpdaterSerivce;
            _fileStorageHelper = fileStorageHelper;
        }

        public async Task<PackageUpdateResponse> CheckForApplicationUpdate()
        {
            try
            {
#if !UNPACKAGED
                var updateRequest = new PackageUpdateRequest
                {
                    PackageArchitecture = Package.Current.Id.Architecture.ToProcessorArchitecture(),
                    PackageName = Package.Current.Id.Name,
                    PackageType = GithubPackageUpdater.Enums.UpdatePackageType.Msix,
                    PackageVersion = Package.Current.Id.Version.ToVersion()
                };
#else
                var updateRequest = new PackageUpdateRequest
                {
                    PackageArchitecture = System.Reflection.Assembly.GetExecutingAssembly().GetName().ProcessorArchitecture.ToProcessorArchitecture(),
                    PackageName = ApplicationPackageName,
                    PackageType = GithubPackageUpdater.Enums.UpdatePackageType.Zip,
                    PackageVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
                };
#endif
                return await _githubPackageUpdaterSerivce.CheckForUpdates(updateRequest);
            }
            catch (Exception updateException)
            {
                _logger.LogError(updateException, "Checking for updates failed with error:");
                return default;
            }
        }

        public async Task ApplyApplicationUpdate(PackageUpdateResponse packageUpdate)
        {

        }
    }
}
