using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI;
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
using WingetGUIInstaller.Utils;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class ListPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly PackageCache _packageCache;
        private readonly PackageManager _packageManager;
        private readonly INavigationService<NavigationItemKey> _navigationService;
        private readonly ObservableCollection<WingetPackageViewModel> _packages;
        private bool _isLoading;
        private WingetPackageViewModel _selectedPackage;
        private string _filterText;
        private string _loadingText;
        private PackageDetailsViewModel _selectedPackageDetails;
        private bool _detailsAvailable;
        private AdvancedCollectionView _packagesView;
        private bool _detailsLoading;

        public ListPageViewModel(DispatcherQueue dispatcherQueue,
            PackageCache packageCache, PackageManager packageManager, INavigationService<NavigationItemKey> navigationService)
        {
            _dispatcherQueue = dispatcherQueue;
            _packageCache = packageCache;
            _packageManager = packageManager;
            _navigationService = navigationService;
            _packages = new ObservableCollection<WingetPackageViewModel>();
            _packages.CollectionChanged += Packages_CollectionChanged;
            PackagesView = new AdvancedCollectionView(_packages, true);
            _ = LoadInstalledPackages();
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

        public AdvancedCollectionView PackagesView
        {
            get => _packagesView;
            set => SetProperty(ref _packagesView, value);
        }

        public PackageDetailsViewModel SelectedPackageDetails
        {
            get => _selectedPackageDetails;
            set => SetProperty(ref _selectedPackageDetails, value);
        }

        public bool DetailsAvailable
        {
            get => _detailsAvailable;
            private set => SetProperty(ref _detailsAvailable, value);
        }

        public bool DetailsLoading
        {
            get => _detailsLoading;
            private set => SetProperty(ref _detailsLoading, value);
        }

        public WingetPackageViewModel SelectedPackage
        {
            get => _selectedPackage;
            set
            {
                if (SetProperty(ref _selectedPackage, value))
                {
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(IsSomethingSelected));
                    _ = FetchPackageDetailsAsync(value);
                }
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    ApplyPackageFilter(value);
                }
            }
        }

        public int SelectedCount => _packages.Any(p => p.IsSelected) ?
            _packages.Count(p => p.IsSelected) : SelectedPackage != default ? 1 : 0;

        public bool IsSomethingSelected => SelectedCount > 0;

        public ICommand ListCommand => new AsyncRelayCommand(() =>
            LoadInstalledPackages(true));

        public ICommand UpgradeSelectedCommand => new AsyncRelayCommand(() =>
             UpgradePackages(_packages.Where(p => p.IsSelected).Select(p => p.Id)));

        public ICommand UninstallSelectedCommand => new AsyncRelayCommand(() =>
            UninstallPackages(_packages.Where(p => p.IsSelected).Select(p => p.Id)));

        public ICommand GoToDetailsCommand =>
            new RelayCommand<PackageDetailsViewModel>(ViewPackageDetails);



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
                UpdateDisplayedPackages(returnedPackages);
                IsLoading = false;
            });
        }

        private void UpdateDisplayedPackages(IEnumerable<WingetPackageEntry> packageEntries)
        {
            _packages.Clear();
            foreach (var entry in packageEntries)
            {
                _packages.Add(new WingetPackageViewModel(entry));
            }
        }

        private async Task UpgradePackages(IEnumerable<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            foreach (var id in packageIds)
            {
                var upgradeResult = await _packageManager.UpgradePackage(id, OnPackageInstallProgress);
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            await LoadInstalledPackages(true);
        }

        private async Task UninstallPackages(IEnumerable<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            foreach (var id in packageIds)
            {
                var uninstallResult = await _packageManager.RemovePackage(id, OnPackageInstallProgress);
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            await LoadInstalledPackages(true);
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
            OnPropertyChanged(nameof(IsSomethingSelected));
        }

        private void ViewPackageDetails(PackageDetailsViewModel details)
        {
            var upgradeOperation = !string.IsNullOrEmpty(_packages.FirstOrDefault(p => p.Id == details.PackageId)?.Available) ?
                AvailableOperation.Update : AvailableOperation.None;

            _navigationService.Navigate(NavigationItemKey.PackageDetails, new PackageDetailsNavigationArgs
            {
                PackageDetails = details,
                AvailableOperation = AvailableOperation.Uninstall | upgradeOperation
            });
        }

        private void OnPackagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WingetPackageViewModel.IsSelected):
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(IsSomethingSelected));
                    _ = FetchPackageDetailsAsync(SelectedPackage);
                    break;
                default:
                    break;
            }
        }

        private void ApplyPackageFilter(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                PackagesView.ClearFiltering();
            }

            PackagesView.ApplyFiltering<WingetPackageViewModel>(package =>
                package.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                || package.Id.Contains(query, StringComparison.InvariantCultureIgnoreCase)
            );
        }
    }
}
