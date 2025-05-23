﻿using GithubPackageUpdater.Models;
using GithubPackageUpdater.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GithubPackageUpdater.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGithubUpdater(
            this IServiceCollection services, Action<PackageUpdaterOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions);

            services.AddOptions<PackageUpdaterOptions>().Configure(configureOptions);
            services.AddSingleton<GithubPackageUpdaterSerivce>();
            return services;
        }
    }
}
