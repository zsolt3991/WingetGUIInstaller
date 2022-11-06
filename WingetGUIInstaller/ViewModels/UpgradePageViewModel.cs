using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class UpgradePageViewModel : ObservableObject, IRecipient<ExclusionListUpdatedMessage>,
        IRecipient<ExclusionStatusChangedMessage>
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ToastNotificationManager _notificationManager;
        private readonly PackageCache _packageCache;
        private readonly PackageManager _packageManager;
        private readonly ExclusionsManager _exclusionsManager;
        private readonly INavigationService<NavigationItemKey> _navigationService;
        private readonly ObservableCollection<WingetPackageViewModel> _packages;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _filterText;

        [ObservableProperty]
        private string _loadingText;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ViewPackageDetailsCommand))]
        private bool _detailsAvailable;

        [ObservableProperty]
        private bool _detailsLoading;

        [ObservableProperty]
        private AdvancedCollectionView _packagesView;

        [ObservableProperty]
        private PackageDetailsViewModel _selectedPackageDetails;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedCount))]
        [NotifyPropertyChangedFor(nameof(CanUpgradeSelected))]
        [NotifyCanExecuteChangedFor(nameof(UpgradeSelectedPackagesCommand))]
        private WingetPackageViewModel _selectedPackage;

        public UpgradePageViewModel(DispatcherQueue dispatcherQueue, PackageCache packageCache, PackageManager packageManager,
            ExclusionsManager exclusionsManager, ToastNotificationManager notificationManager,
            INavigationService<NavigationItemKey> navigationService)
        {
            _dispatcherQueue = dispatcherQueue;
            _packageCache = packageCache;
            _packageManager = packageManager;
            _exclusionsManager = exclusionsManager;
            _notificationManager = notificationManager;
            _navigationService = navigationService;
            _packages = new ObservableCollection<WingetPackageViewModel>();
            _packages.CollectionChanged += Packages_CollectionChanged;
            PackagesView = new AdvancedCollectionView(_packages, true);
            WeakReferenceMessenger.Default.RegisterAll(this);
            _ = ListUpgradableItemsAsync();
        }

        public int SelectedCount => _packages.Any(p => p.IsSelected) ?
            _packages.Count(p => p.IsSelected) : SelectedPackage != default ? 1 : 0;

        public bool CanUpgradeAll => _packages.Any();

        public bool CanUpgradeSelected => SelectedCount > 0;

        [RelayCommand(CanExecute = nameof(CanUpgradeSelected))]
        private async Task UpgradeSelectedPackages()
        {
            await UpgradePackagesAsync(GetSelectedPackageIds());
        }

        [RelayCommand(CanExecute = nameof(CanUpgradeAll))]
        private async Task UpgradeAllPackages()
        {
            await UpgradePackagesAsync(_packages.Select(p => p.Id));
        }

        [RelayCommand]
        private async Task RefreshPackageList()
        {
            await ListUpgradableItemsAsync(true);
        }

        [RelayCommand(CanExecute = nameof(DetailsAvailable))]
        private void ViewPackageDetails(PackageDetailsViewModel obj)
        {
            _navigationService.Navigate(NavigationItemKey.PackageDetails, new PackageDetailsNavigationArgs
            {
                PackageDetails = obj,
                AvailableOperation = AvailableOperation.Uninstall | AvailableOperation.Update
            });
        }

        partial void OnSelectedPackageChanged(WingetPackageViewModel value)
        {
            _ = FetchPackageDetailsAsync(value);
        }

        partial void OnFilterTextChanged(string value)
        {
            FilterPackageList(value, _exclusionsManager.ExcludedPackagesEnabled);
        }

        private async Task ListUpgradableItemsAsync(bool forceReload = false)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = true;
                LoadingText = "Loading";
            });

            var returnedPackages = await _packageCache.GetUpgradablePackages(forceReload);

            _dispatcherQueue.TryEnqueue(() =>
            {
                _packages.Clear();
                foreach (var entry in returnedPackages)
                {
                    _packages.Add(new WingetPackageViewModel(entry));
                }
                FilterPackageList(_filterText, _exclusionsManager.ExcludedPackagesEnabled);
                IsLoading = false;
                _notificationManager.ShowUpdateStatus(_packagesView.Count);
            });
        }

        private async Task UpgradePackagesAsync(IEnumerable<string> packageIds)
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
            }

            if (packageIds.Any())
            {
                _notificationManager.ShowBatchPackageOperationStatus(
                    InstallOperation.Upgrade, packageIds.Count(), successfulInstalls);
            }

            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            await ListUpgradableItemsAsync(true);
        }


        private async Task FetchPackageDetailsAsync(WingetPackageViewModel value)
        {
            // Clear the displayed details on multiple items being selected
            if (_packages.Any(p => p.IsSelected && p.Id != value.Id))
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    DetailsAvailable = false;
                    DetailsLoading = false;
                });
                return;
            }

            // Clear the details if value is null
            if (value == default)
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    DetailsAvailable = false;
                    DetailsLoading = false;
                });
                return;
            }

            _dispatcherQueue.TryEnqueue(() =>
            {
                DetailsAvailable = false;
                DetailsLoading = true;
            });

            var details = await _packageCache.GetPackageDetails(value.Id);
            if (details != default)
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    SelectedPackageDetails = new PackageDetailsViewModel(details);
                    DetailsAvailable = true;
                    DetailsLoading = false;
                });
            }
            else
            {
                _dispatcherQueue.TryEnqueue(() => DetailsLoading = false);
            }
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
            UpgradeSelectedPackagesCommand.NotifyCanExecuteChanged();
            UpgradeAllPackagesCommand.NotifyCanExecuteChanged();
        }

        private void OnPackagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WingetPackageViewModel.IsSelected):
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(CanUpgradeSelected));
                    UpgradeSelectedPackagesCommand.NotifyCanExecuteChanged();
                    _ = FetchPackageDetailsAsync(SelectedPackage);
                    break;
                default:
                    break;
            }
        }

        private IEnumerable<string> GetSelectedPackageIds()
        {
            // Prioritize Selected Items 
            if (_packages.Any(p => p.IsSelected))
            {
                return _packages.Where(p => p.IsSelected).Select(p => p.Id);
            }

            // Use the highlighted item if there is no selection
            if (_selectedPackage != default)
            {
                return new List<string>() { _selectedPackage.Id };
            }

            return new List<string>();
        }

        private void FilterPackageList(string filterText, bool hideExcluded)
        {
            if (string.IsNullOrWhiteSpace(filterText) && hideExcluded == false)
            {
                PackagesView.ClearFiltering();
                return;
            }

            if (string.IsNullOrEmpty(filterText))
            {
                PackagesView.ApplyFiltering<WingetPackageViewModel>(package =>
                    !_exclusionsManager.IsPackageExcluded(package.Id));
                return;
            }

            PackagesView.ApplyFiltering<WingetPackageViewModel>(package =>
                (package.Name.Contains(filterText, StringComparison.InvariantCultureIgnoreCase)
                || package.Id.Contains(filterText, StringComparison.InvariantCultureIgnoreCase))
                && !_exclusionsManager.IsPackageExcluded(package.Id));
        }

        void IRecipient<ExclusionStatusChangedMessage>.Receive(ExclusionStatusChangedMessage message)
        {
            FilterPackageList(_filterText, _exclusionsManager.ExcludedPackagesEnabled);
        }

        void IRecipient<ExclusionListUpdatedMessage>.Receive(ExclusionListUpdatedMessage message)
        {
            FilterPackageList(_filterText, _exclusionsManager.ExcludedPackagesEnabled);
        }
    }
}
