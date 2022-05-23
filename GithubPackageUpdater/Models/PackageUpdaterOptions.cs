using Microsoft.Extensions.Logging;

namespace GithubPackageUpdater.Models
{
    public class PackageUpdaterOptions
    {
        public string RepositoryName { get; private set; }
        public string AccountName { get; private set; }
        public string AccessToken { get; private set; }

        internal PackageUpdaterOptions()
        { }

        public PackageUpdaterOptions(string accountName, string repositoryName, string accessToken = default)
        {
            AccountName = accountName;
            RepositoryName = repositoryName;
            AccessToken = accessToken;
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
    }
}
