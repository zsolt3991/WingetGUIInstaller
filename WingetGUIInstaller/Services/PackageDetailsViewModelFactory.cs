using Microsoft.Extensions.DependencyInjection;
using System;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.ViewModels;
using WingetHelper.Models;

namespace WingetGUIInstaller.Services
{
    internal sealed class PackageDetailsViewModelFactory : IPackageDetailsViewModelFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PackageDetailsViewModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public PackageDetailsViewModel GetPackageDetailsViewModel(WingetPackageDetails packageDetails)
        {
            return ActivatorUtilities.CreateInstance<PackageDetailsViewModel>(_serviceProvider, packageDetails);
        }
    }
}
