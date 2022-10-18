using System;

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
            AccountName = accountName ?? throw new ArgumentNullException(nameof(accountName));
            RepositoryName = repositoryName ?? throw new ArgumentNullException(nameof(repositoryName));
            AccessToken = accessToken;
        }

        public PackageUpdaterOptions ConfigureAccountName(string accountName)
        {
            AccountName = accountName ?? throw new ArgumentNullException(nameof(accountName));
            return this;
        }

        public PackageUpdaterOptions ConfigureRepository(string repositoryName)
        {
            RepositoryName = repositoryName ?? throw new ArgumentNullException(nameof(repositoryName));
            return this;
        }

        public PackageUpdaterOptions ConfigureAuthorization(string accessToken)
        {
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            return this;
        }
    }
}
