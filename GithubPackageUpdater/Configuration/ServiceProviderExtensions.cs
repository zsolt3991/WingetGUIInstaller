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
            var options = new PackageUpdaterOptions();
            configuration.Invoke(options);

            services.AddSingleton(provider => ActivatorUtilities.CreateInstance<GithubPackageUpdaterSerivce>(provider, options));
            return services;
        }
    }
}
