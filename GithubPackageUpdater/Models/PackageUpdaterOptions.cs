using GithubPackageUpdater.Utils;
using Microsoft.Extensions.Logging;

namespace GithubPackageUpdater.Models
{
    public class PackageUpdaterOptions
    {
        public string RepositoryName { get; private set; }
        public string AccountName { get; private set; }
        public string AccessToken { get; private set; }
        public ILogger Logger { get; private set; }

        internal PackageUpdaterOptions()
        {
            Logger = new DebugLogger();
        }

        public PackageUpdaterOptions(string accountName, string repositoryName, string accessToken = default, ILogger logger = default)
        {
            AccountName = accountName;
            RepositoryName = repositoryName;
            AccessToken = accessToken;
            Logger = logger != default ? logger : new DebugLogger();
        }

        public PackageUpdaterOptions ConfigureAccountName(string accountName)
        {
            AccountName = accountName;
            return this;
        }

        public PackageUpdaterOptions ConfigureRepository(string repositoryName)
        {
            RepositoryName = repositoryName;
            return this;
        }

        public PackageUpdaterOptions ConfigureAuthorization(string accessToken)
        {
            AccessToken = accessToken;
            return this;
        }

        public PackageUpdaterOptions ConfigureLogging(ILogger logger)
        {
            if (logger != default)
            {
                Logger = logger;
            }
            return this;
        }

        public PackageUpdaterOptions ConfigureLogging(ILoggerFactory loggerFactory)
        {
            if (loggerFactory != default)
            {
                Logger = loggerFactory.CreateLogger<Services.GithubPackageUpdaterSerivce>();
            }
            return this;
        }
    }
}
