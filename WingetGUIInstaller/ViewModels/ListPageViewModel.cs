using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;
using WingetHelper.Enums;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class ListPageViewModel : ObservableObject,
        IRecipient<ExclusionListUpdatedMessage>,
        IRecipient<ExclusionStatusChangedMessage>,
        IRecipient<FilterSourcesListUpdatedMessage>,
        IRecipient<FilterSourcesStatusChangedMessage>,
        IRecipient<IgnoreEmptySourcesStatusChangedMessage>,
        INavigationAware
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly PackageCache _packageCache;
        private readonly PackageManager _packageManager;
        private readonly ToastNotificationManager _notificationManager;
        private readonly INavigationService<NavigationItemKey> _navigationService;
        private readonly PackageDetailsCache _packageDetailsCache;
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
        [NotifyPropertyChangedFor(nameof(SelectedCount))]
        [NotifyPropertyChangedFor(nameof(IsSomethingSelected))]
        [NotifyPropertyChangedFor(nameof(IsSelectionUpgradable))]
        [NotifyCanExecuteChangedFor(nameof(UpgradePackagesCommand))]
        [NotifyCanExecuteChangedFor(nameof(UninstallPackagesCommand))]
        private WingetPackageViewModel _selectedPackage;

        [ObservableProperty]
        private PackageDetailsViewModel _selectedPackageDetails;

        public ListPageViewModel(DispatcherQueue dispatcherQueue,
            PackageCache packageCache, PackageManager packageManager, ToastNotificationManager notificationManager,
            INavigationService<NavigationItemKey> navigationService, PackageDetailsCache packageDetailsCache)
        {
            _dispatcherQueue = dispatcherQueue;
            _packageCache = packageCache;
            _packageManager = packageManager;
            _notificationManager = notificationManager;
            _navigationService = navigationService;
            _packageDetailsCache = packageDetailsCache;
            _packages = new ObservableCollection<WingetPackageViewModel>();
            _packages.CollectionChanged += Packages_CollectionChanged;
            PackagesView = new AdvancedCollectionView(_packages, true);
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public int SelectedCount => _packages.Any(p => p.IsSelected) ?
            _packages.Count(p => p.IsSelected) : SelectedPackage != default ? 1 : 0;

        public bool IsSomethingSelected => SelectedCount > 0;

        public bool IsSelectionUpgradable => _packages.Any(p => p.IsSelected) ?
            _packages.Where(p => p.IsSelected).Any(p => !string.IsNullOrEmpty(p.Available)) :
            SelectedPackage != default && !string.IsNullOrEmpty(SelectedPackage.Available);

        public void OnNavigatedTo(object parameter)
        {
            _ = LoadInstalledPackages(false);
        }

        public void OnNavigatedFrom(NavigationMode navigationMode)
        { }

        [RelayCommand]
        private async Task RefreshPackageList()
        {
            await LoadInstalledPackages(true);
        }

        [RelayCommand(CanExecute = nameof(DetailsAvailable))]
        private void ViewPackageDetails(string packageId)
        {
            var upgradeOperation = !string.IsNullOrEmpty(_packages.FirstOrDefault(p => p.Id == packageId)?.Available) ?
                AvailableOperation.Update : AvailableOperation.None;

            _navigationService.Navigate(NavigationItemKey.PackageDetails, args: new PackageDetailsNavigationArgs
            {
                PackageId = packageId,
                AvailableOperation = AvailableOperation.Uninstall | upgradeOperation
            });
        }

        [RelayCommand(CanExecute = nameof(IsSelectionUpgradable))]
        private async Task UpgradePackages()
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(false));

            var successfulInstalls = 0;
            var packageIds = GetSelectedPackageIds();
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
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(true));
            await LoadInstalledPackages(true);
        }

        [RelayCommand(CanExecute = nameof(IsSomethingSelected))]
        private async Task UninstallPackages()
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(false));

            var successfulInstalls = 0;
            var packageIds = GetSelectedPackageIds();
            foreach (var id in packageIds)
            {
                var uninstallResult = await _packageManager.RemovePackage(id, OnPackageInstallProgress);
                if (uninstallResult)
                {
                    successfulInstalls++;
                }
            }

            if (packageIds.Any())
            {
                _notificationManager.ShowBatchPackageOperationStatus(
                    InstallOperation.Uninstall, packageIds.Count(), successfulInstalls);
            }

            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(true));
            await LoadInstalledPackages(true);
        }

        partial void OnSelectedPackageChanged(WingetPackageViewModel value)
        {
            _ = FetchPackageDetailsAsync(value);
        }

        partial void OnFilterTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                PackagesView.ClearFiltering();
            }

            PackagesView.ApplyFiltering<WingetPackageViewModel>(package =>
                package.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase)
                || package.Id.Contains(value, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        private async Task LoadInstalledPackages(bool forceUpdate = false)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = true;
                LoadingText = "Loading";
            });

            var returnedPackages = await _packageCache.GetInstalledPackages(forceUpdate);

            _dispatcherQueue.TryEnqueue(() =>
            {
                _packages.Clear();
                foreach (var entry in returnedPackages)
                {
                    _packages.Add(new WingetPackageViewModel(entry));
                }
                IsLoading = false;
            });
        }

        private async Task FetchPackageDetailsAsync(WingetPackageViewModel value)
        {
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

            _dispatcherQueue.TryEnqueue(() =>
            {
                DetailsAvailable = false;
                DetailsLoading = true;
            });

            var details = await _packageDetailsCache.GetPackageDetails(value.Id);
            if (details != default)
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    SelectedPackageDetails = new PackageDetailsViewModel(details, _navigationService);
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
            OnPropertyChanged(nameof(IsSomethingSelected));
            OnPropertyChanged(nameof(IsSelectionUpgradable));
            UpgradePackagesCommand.NotifyCanExecuteChanged();
            UninstallPackagesCommand.NotifyCanExecuteChanged();
        }

        private void OnPackagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WingetPackageViewModel.IsSelected):
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(IsSomethingSelected));
                    OnPropertyChanged(nameof(IsSelectionUpgradable));
                    UpgradePackagesCommand.NotifyCanExecuteChanged();
                    UninstallPackagesCommand.NotifyCanExecuteChanged();
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
            if (SelectedPackage != default)
            {
                return new List<string>() { SelectedPackage.Id };
            }

            return new List<string>();
        }

        void IRecipient<IgnoreEmptySourcesStatusChangedMessage>.Receive(IgnoreEmptySourcesStatusChangedMessage message)
        {
            _ = LoadInstalledPackages(false);
        }

        void IRecipient<FilterSourcesStatusChangedMessage>.Receive(FilterSourcesStatusChangedMessage message)
        {
            _ = LoadInstalledPackages(false);
        }

        void IRecipient<FilterSourcesListUpdatedMessage>.Receive(FilterSourcesListUpdatedMessage message)
        {
            _ = LoadInstalledPackages(false);
        }

        void IRecipient<ExclusionStatusChangedMessage>.Receive(ExclusionStatusChangedMessage message)
        {
            _ = LoadInstalledPackages(false);
        }

        void IRecipient<ExclusionListUpdatedMessage>.Receive(ExclusionListUpdatedMessage message)
        {
            _ = LoadInstalledPackages(false);
        }
    }
}
