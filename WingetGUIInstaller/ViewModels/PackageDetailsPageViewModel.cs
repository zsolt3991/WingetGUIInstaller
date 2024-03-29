﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;
using WingetHelper.Enums;

namespace WingetGUIInstaller.ViewModels
{
    internal sealed partial class PackageDetailsPageViewModel : ObservableObject
    {
        private readonly PackageManager _packageManager;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ToastNotificationManager _toastNotificationManager;
        private readonly PackageDetailsCache _packageDetailsCache;
        private readonly IPackageDetailsViewModelFactory _packageDetailsViewModelFactory;
        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForViewIndependentUse();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsInstallSupported))]
        [NotifyPropertyChangedFor(nameof(IsUpdateSupported))]
        [NotifyPropertyChangedFor(nameof(IsUninstallSupported))]
        private AvailableOperation _availableOperation;

        [ObservableProperty]
        private string _loadingText;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private PackageDetailsViewModel _packageDetails;

        public PackageDetailsPageViewModel(PackageManager packageManager, DispatcherQueue dispatcherQueue,
            ToastNotificationManager toastNotificationManager, PackageDetailsCache packageDetailsCache,
           IPackageDetailsViewModelFactory packageDetailsViewModelFactory)
        {
            _packageManager = packageManager;
            _dispatcherQueue = dispatcherQueue;
            _toastNotificationManager = toastNotificationManager;
            _packageDetailsCache = packageDetailsCache;
            _packageDetailsViewModelFactory = packageDetailsViewModelFactory;
        }

        public bool IsInstallSupported => AvailableOperation.HasFlag(AvailableOperation.Install);

        public bool IsUpdateSupported => AvailableOperation.HasFlag(AvailableOperation.Update);

        public bool IsUninstallSupported => AvailableOperation.HasFlag(AvailableOperation.Uninstall);

        public void UpdateData(PackageDetailsNavigationArgs packageDetails)
        {
            if (string.IsNullOrEmpty(packageDetails.PackageId))
            {
                throw new Exception("Unexpected input");
            }

            _ = FetchPackageDetailsAsync(packageDetails.PackageId);
            AvailableOperation = packageDetails.AvailableOperation;
        }

        [RelayCommand]
        private async Task InstallPackageAsync(string packageId)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(false));

            var installresult = await _packageManager.InstallPacakge(packageId, OnPackageInstallProgress);
            if (installresult)
            {
                AvailableOperation = AvailableOperation &= ~AvailableOperation.Install;
            }

            _toastNotificationManager.ShowPackageOperationStatus(PackageDetails.PackageName, InstallOperation.Install, installresult);
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = false;
                LoadingText = _resourceLoader.GetString("LoadingText");
            });
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(true));
        }

        [RelayCommand]
        private async Task UpgradePackageAsync(string packageId)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(false));

            var upgradeResult = await _packageManager.UpgradePackage(packageId, OnPackageInstallProgress);
            if (upgradeResult)
            {
                AvailableOperation = AvailableOperation &= ~AvailableOperation.Update;
            }

            _toastNotificationManager.ShowPackageOperationStatus(PackageDetails.PackageName, InstallOperation.Upgrade, upgradeResult);
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = false;
                LoadingText = _resourceLoader.GetString("LoadingText");
            });
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(true));
        }

        [RelayCommand]
        private async Task UninstallPackageAsync(string packageId)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(false));

            var uninstallResult = await _packageManager.RemovePackage(packageId, OnPackageInstallProgress);
            if (uninstallResult)
            {
                AvailableOperation = AvailableOperation &= ~AvailableOperation.Uninstall;
            }

            _toastNotificationManager.ShowPackageOperationStatus(PackageDetails.PackageName, InstallOperation.Uninstall, uninstallResult);
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = false;
                LoadingText = _resourceLoader.GetString("LoadingText");
            });
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(true));
        }


        private async Task FetchPackageDetailsAsync(string packageId)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                LoadingText = _resourceLoader.GetString("LoadingText");
                IsLoading = true;
            });

            var details = await _packageDetailsCache.GetPackageDetails(packageId);

            _dispatcherQueue.TryEnqueue(() =>
            {
                PackageDetails = _packageDetailsViewModelFactory.GetPackageDetailsViewModel(details);
                IsLoading = false;
            });
        }

        private void OnPackageInstallProgress(WingetProcessState progess)
        {
            _dispatcherQueue.TryEnqueue(() => LoadingText = _resourceLoader.GetString(progess.GetResourceKey()));
        }
    }
}
