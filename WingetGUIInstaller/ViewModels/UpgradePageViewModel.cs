using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class UpgradePageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ToastNotificationManager _notificationManager;
        private readonly PackageCache _packageCache;
        private readonly PackageManager _packageManager;
        private readonly INavigationService<NavigationItemKey> _navigationService;
        private ObservableCollection<WingetPackageViewModel> _packages;
        private bool _isLoading;
        private WingetPackageViewModel _selectedPackage;
        private string _filterText;
        private List<WingetPackageEntry> _returnedPackages;
        private string _loadingText;
        private PackageDetailsViewModel _selectedPackageDetails;
        private bool _isDetailsAvailable;

        public UpgradePageViewModel(DispatcherQueue dispatcherQueue, PackageCache packageCache, PackageManager packageManager, 
            ToastNotificationManager notificationManager, INavigationService<NavigationItemKey> navigationService)
        {
            _dispatcherQueue = dispatcherQueue;
            _packageCache = packageCache;
            _packageManager = packageManager;
            _notificationManager = notificationManager;
            _navigationService = navigationService;
            _packages = new ObservableCollection<WingetPackageViewModel>();
            Packages.CollectionChanged += Packages_CollectionChanged;
            _ = ListUpgradableItemsAsync();
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

        public ObservableCollection<WingetPackageViewModel> Packages
        {
            get => _packages;
            set => SetProperty(ref _packages, value);
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    UpdateDisplayedPackages();
                }
            }
        }

        public PackageDetailsViewModel SelectedPackageDetails
        {
            get => _selectedPackageDetails;
            set => SetProperty(ref _selectedPackageDetails, value);
        }

        public WingetPackageViewModel SelectedPackage
        {
            get => _selectedPackage;
            set
            {
                if (SetProperty(ref _selectedPackage, value))
                {
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(CanUpgradeSelected));
                    _ = FetchPackageDetailsAsync(value);
                }
            }
        }

        public int SelectedCount => Packages.Any(p => p.IsSelected) ?
            Packages.Count(p => p.IsSelected) : SelectedPackage != default ? 1 : 0;

        public bool CanUpgradeAll => Packages.Any();

        public bool CanUpgradeSelected => SelectedCount > 0;

        public bool DetailsAvailable
        {
            get => _isDetailsAvailable;
            private set => SetProperty(ref _isDetailsAvailable, value);
        }

        public ICommand RefreshCommand => new AsyncRelayCommand(()
            => ListUpgradableItemsAsync(true));

        public ICommand UpgradeSelectedCommand => new AsyncRelayCommand(() =>
            UpgradePackagesAsync(Packages.Where(p => p.IsSelected).Select(p => p.Id).ToList()));

        public ICommand UpgradeAllCommand => new AsyncRelayCommand(() =>
            UpgradePackagesAsync(Packages.Select(p => p.Id).ToList()));

        public ICommand GoToDetailsCommand =>
            new RelayCommand<PackageDetailsViewModel>(ViewPackageDetails);

        private async Task ListUpgradableItemsAsync(bool forceReload = false)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = true;
                LoadingText = "Loading";
            });

            _returnedPackages = await _packageCache.GetUpgradablePackages(forceReload);

            _dispatcherQueue.TryEnqueue(() =>
            {
                UpdateDisplayedPackages();
                IsLoading = false;
            });

            _notificationManager.ShowUpdateStatus(_returnedPackages.Count != 0, _returnedPackages.Count);
        }

        private void UpdateDisplayedPackages()
        {
            Packages.Clear();
            var filteredResult =
                !string.IsNullOrWhiteSpace(FilterText)
                ? _returnedPackages.Where(p => p.Name.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase)
                    || p.Id.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase))
                : _returnedPackages;

            foreach (var entry in filteredResult)
            {
                Packages.Add(new WingetPackageViewModel(entry));
            }
        }

        private async Task UpgradePackagesAsync(List<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            var successfulInstalls = 0;

            foreach (var id in packageIds)
            {                
                var upgradeResult = await _packageManager.UpgradePackage(id, OnPackageInstallProgress);

                if (upgradeResult)
                {
                    successfulInstalls++;
                }

                if (packageIds.Count == 1)
                {
                    _notificationManager.ShowPackageOperationStatus(
                        _returnedPackages.Find(p => p.Id == id)?.Name, InstallOperation.Upgrade, upgradeResult);
                }
            }

            if (packageIds.Count != 1)
            {
                _notificationManager.ShowMultiplePackageOperationStatus(
                    InstallOperation.Upgrade, successfulInstalls, packageIds.Count - successfulInstalls);
            }

            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            await ListUpgradableItemsAsync(true);
        }


        private async Task FetchPackageDetailsAsync(WingetPackageViewModel value)
        {
            if (Packages.Any(p => p.IsSelected))
            {
                return;
            }

            if (value != default)
            {
                _dispatcherQueue.TryEnqueue(() => DetailsAvailable = false);

                var details = await _packageCache.GetPackageDetails(value.Id);
                _dispatcherQueue.TryEnqueue(() => SelectedPackageDetails = new PackageDetailsViewModel(details));
                _dispatcherQueue.TryEnqueue(() => DetailsAvailable = true);
            }
            else
            {
                _dispatcherQueue.TryEnqueue(() => DetailsAvailable = false);
            }
        }

        private void ViewPackageDetails(PackageDetailsViewModel obj)
        {
            _navigationService.Navigate(NavigationItemKey.PackageDetails, new PackageDetailsNavigationArgs
            {
                PackageDetails = obj,
                AvailableOperation = AvailableOperation.Uninstall | AvailableOperation.Update
            });
        }

        private void OnPackageInstallProgress(WingetProcessState progess)
        {
            _dispatcherQueue.TryEnqueue(() => LoadingText = progess.ToString());
        }

        private void Packages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != default)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is WingetPackageViewModel packageViewModel)
                    {
                        packageViewModel.PropertyChanged += OnPackagePropertyChanged;
                    }
                }
            }

            if ((e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace) && e.OldItems != default)
            {
                foreach (var item in e.OldItems)
                {

                    if (item is WingetPackageViewModel packageViewModel)
                    {
                        packageViewModel.PropertyChanged -= OnPackagePropertyChanged;
                    }
                }
            }

            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(CanUpgradeSelected));
            OnPropertyChanged(nameof(CanUpgradeAll));
        }

        private void OnPackagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WingetPackageViewModel.IsSelected):
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(CanUpgradeSelected));
                    _ = FetchPackageDetailsAsync(SelectedPackage);
                    break;
                default:
                    break;
            }
        }
    }
}
