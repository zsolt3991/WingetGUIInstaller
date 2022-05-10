using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    internal class PackageDetailsPageViewModel : ObservableObject
    {
        private readonly PackageManager _packageManager;
        private readonly PackageCache _packageCache;
        private readonly DispatcherQueue _dispatcherQueue;
        private PackageDetailsViewModel _packageDetails;
        private AvailableOperation _availableOperation;
        private string _loadingText;
        private bool _isLoading;

        public PackageDetailsPageViewModel(PackageManager packageManager, PackageCache packageCache, DispatcherQueue dispatcherQueue)
        {
            _packageManager = packageManager;
            _packageCache = packageCache;
            _dispatcherQueue = dispatcherQueue;
        }

        public PackageDetailsViewModel PackageDetails
        {
            get => _packageDetails;
            set => SetProperty(ref _packageDetails, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string LoadingText
        {
            get => _loadingText;
            set => SetProperty(ref _loadingText, value);
        }

        public bool IsInstallSupported => _availableOperation.HasFlag(AvailableOperation.Install);

        public bool IsUpdateSupported => _availableOperation.HasFlag(AvailableOperation.Update);

        public bool IsUninstallSupported => _availableOperation.HasFlag(AvailableOperation.Uninstall);

        public ICommand InstallCommand => new AsyncRelayCommand<string>(InstallPackageAsync);

        public ICommand UpdateCommand => new AsyncRelayCommand<string>(UpgradePackageAsync);

        public ICommand UninstallCommand => new AsyncRelayCommand<string>(UninstallPackageAsync);

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
            _availableOperation = packageDetails.AvailableOperation;
            OnPropertyChanged(nameof(IsInstallSupported));
            OnPropertyChanged(nameof(IsUninstallSupported));
            OnPropertyChanged(nameof(IsUpdateSupported));
        }

        private async Task InstallPackageAsync(string packageId)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            var installresult = await _packageManager.InstallPacakge(packageId, OnPackageInstallProgress);
            if (installresult)
            {
                _availableOperation = _availableOperation &= ~AvailableOperation.Install;
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            _dispatcherQueue.TryEnqueue(() => LoadingText = "Loading");
        }

        private async Task UpgradePackageAsync(string packageId)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            var upgradeResult = await _packageManager.UpgradePackage(packageId, OnPackageInstallProgress);
            if (upgradeResult)
            {
                _availableOperation = _availableOperation &= ~AvailableOperation.Update;
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            _dispatcherQueue.TryEnqueue(() => LoadingText = "Loading");
        }

        private async Task UninstallPackageAsync(string packageId)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            var uninstallResult = await _packageManager.RemovePackage(packageId, OnPackageInstallProgress);
            if (uninstallResult)
            {
                _availableOperation = _availableOperation &= ~AvailableOperation.Uninstall;
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            _dispatcherQueue.TryEnqueue(() => LoadingText = "Loading");
        }

        private async Task FetchPackageDetailsAsync(string packageId)
        {
            _dispatcherQueue.TryEnqueue(() => LoadingText = "Loading");
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            var details = await _packageCache.GetPackageDetails(packageId);
            _dispatcherQueue.TryEnqueue(() => PackageDetails = new PackageDetailsViewModel(details));
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
        }

        private void OnPackageInstallProgress(WingetProcessState progess)
        {
            _dispatcherQueue.TryEnqueue(() => LoadingText = progess.ToString());
        }
    }
}
