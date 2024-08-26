using System;

namespace GithubPackageUpdater.Models
{
    public sealed class PackageUpdateException : Exception
    {
        public PackageUpdateException(string message) : base(message)
        {
        }

        public PackageUpdateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}