using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    internal partial class PackageDetailsPageViewModel : ObservableObject
    {
        private readonly PackageManager _packageManager;
        private readonly PackageCache _packageCache;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ToastNotificationManager _toastNotificationManager;

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

        public PackageDetailsPageViewModel(PackageManager packageManager, PackageCache packageCache, DispatcherQueue dispatcherQueue,
            ToastNotificationManager toastNotificationManager)
        {
            _packageManager = packageManager;
            _packageCache = packageCache;
            _dispatcherQueue = dispatcherQueue;
            _toastNotificationManager = toastNotificationManager;
        }

        public bool IsInstallSupported => _availableOperation.HasFlag(AvailableOperation.Install);

        public bool IsUpdateSupported => _availableOperation.HasFlag(AvailableOperation.Update);

        public bool IsUninstallSupported => _availableOperation.HasFlag(AvailableOperation.Uninstall);

        public void UpdateData(PackageDetailsNavigationArgs packageDetails)
        {
            if (packageDetails.PackageDetails != default)
            {
                PackageDetails = packageDetails.PackageDetails;
            }
            else
            {
                if (!string.IsNullOrEmpty(packageDetails.PackageId))
                {
                    _ = FetchPackageDetailsAsync(packageDetails.PackageId);
                }
                else
                {
                    throw new Exception("Unexpected input");
                }
            }
            AvailableOperation = packageDetails.AvailableOperation;
        }

        [RelayCommand]
        private async Task InstallPackageAsync(string packageId)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);

            var installresult = await _packageManager.InstallPacakge(packageId, OnPackageInstallProgress);
            if (installresult)
            {
                AvailableOperation = AvailableOperation &= ~AvailableOperation.Install;
            }

            _toastNotificationManager.ShowPackageOperationStatus(_packageDetails.PackageName, InstallOperation.Install, installresult);
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = false;
                LoadingText = "Loading";
                OnPropertyChanged(nameof(IsInstallSupported));
            });
        }

        [RelayCommand]
        private async Task UpgradePackageAsync(string packageId)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);

            var upgradeResult = await _packageManager.UpgradePackage(packageId, OnPackageInstallProgress);
            if (upgradeResult)
            {
                AvailableOperation = AvailableOperation &= ~AvailableOperation.Update;
            }

            _toastNotificationManager.ShowPackageOperationStatus(_packageDetails.PackageName, InstallOperation.Upgrade, upgradeResult);
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = false;
                LoadingText = "Loading";
                OnPropertyChanged(nameof(IsUpdateSupported));
            });
        }

        [RelayCommand]
        private async Task UninstallPackageAsync(string packageId)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);

            var uninstallResult = await _packageManager.RemovePackage(packageId, OnPackageInstallProgress);
            if (uninstallResult)
            {
                AvailableOperation = AvailableOperation &= ~AvailableOperation.Uninstall;
            }

            _toastNotificationManager.ShowPackageOperationStatus(_packageDetails.PackageName, InstallOperation.Uninstall, uninstallResult);
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = false;
                LoadingText = "Loading";
                OnPropertyChanged(nameof(IsUninstallSupported));
            });
        }

        private async Task FetchPackageDetailsAsync(string packageId)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                LoadingText = "Loading";
                IsLoading = true;
            });

            var details = await _packageCache.GetPackageDetails(packageId);

            _dispatcherQueue.TryEnqueue(() =>
            {
                PackageDetails = new PackageDetailsViewModel(details);
                IsLoading = false;
            });
        }

        private void OnPackageInstallProgress(WingetProcessState progess)
        {
            _dispatcherQueue.TryEnqueue(() => LoadingText = progess.ToString());
        }
    }
}
