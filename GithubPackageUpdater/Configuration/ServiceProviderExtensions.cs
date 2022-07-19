using GithubPackageUpdater.Models;
using GithubPackageUpdater.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GithubPackageUpdater.Configuration
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddGithubUpdater(
               this IServiceCollection services, Action<PackageUpdaterOptions> configuration)
        {
            if (configuration == default)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var options = new PackageUpdaterOptions();
            configuration.Invoke(options);

            services.AddLogging();
            services.AddSingleton(options);
            services.AddSingleton<GithubPackageUpdaterSerivce>();
            return services;
        }
    }
}
