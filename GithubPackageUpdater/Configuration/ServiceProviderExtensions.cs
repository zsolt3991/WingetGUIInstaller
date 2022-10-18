using GithubPackageUpdater.Models;
using GithubPackageUpdater.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GithubPackageUpdater.Configuration
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddGithubUpdater(
            this IServiceCollection services, Action<PackageUpdaterOptions> configureOptions)
        {
            if (configureOptions == default)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddLogging();
            services.AddOptions<PackageUpdaterOptions>().Configure(configureOptions);
            services.AddSingleton<GithubPackageUpdaterSerivce>();
            return services;
        }
    }
}
