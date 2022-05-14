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
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class SearchPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly PackageCache _packageCache;
        private readonly PackageManager _packageManager;
        private readonly INavigationService<NavigationItemKey> _navigationService;
        private readonly ObservableCollection<WingetPackageViewModel> _packages;
        private bool _isLoading;
        private WingetPackageViewModel _selectedPackage;
        private string _searchQuery;
        private string _loadingText;
        private bool _isDetailsAvailable;
        private PackageDetailsViewModel _selectedPackageDetails;
        private AdvancedCollectionView _packagesView;

        public SearchPageViewModel(DispatcherQueue dispatcherQueue,
            PackageCache packageCache, PackageManager packageManager, INavigationService<NavigationItemKey> navigationService)
        {
            _dispatcherQueue = dispatcherQueue;
            _packageCache = packageCache;
            _packageManager = packageManager;
            _navigationService = navigationService;
            _packages = new ObservableCollection<WingetPackageViewModel>();
            _packages.CollectionChanged += Packages_CollectionChanged;

            PackagesView = new AdvancedCollectionView(_packages, true);
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

        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
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
                    OnPropertyChanged(nameof(CanInstallSelected));
                    _ = FetchPackageDetailsAsync(value);
                }
            }
        }

        public int SelectedCount => _packages.Any(p => p.IsSelected) ? _packages.Count(p => p.IsSelected) : SelectedPackage != default ? 1 : 0;

        public bool CanInstallAll => _packages.Any();

        public bool CanInstallSelected => SelectedCount > 0;

        public bool DetailsAvailable
        {
            get => _isDetailsAvailable;
            private set => SetProperty(ref _isDetailsAvailable, value);
        }

        public ICommand SearchCommand => new AsyncRelayCommand(()
            => SerchPackageAsync(SearchQuery, false));

        public ICommand InstallSelectedCommand => new AsyncRelayCommand(()
            => InstallPackagesAsync(_packages.Where(p => p.IsSelected).Select(p => p.Id)));

        public ICommand InstalAllCommand => new AsyncRelayCommand(()
            => InstallPackagesAsync(_packages.Select(p => p.Id)));

        public ICommand GoToDetailsCommand =>
            new RelayCommand<PackageDetailsViewModel>(ViewPackageDetails);

        private async Task SerchPackageAsync(string searchQuery, bool refreshInstalled = false)
        {
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        PackagesView.Clear();
                        LoadingText = "Loading";
                        IsLoading = true;
                    });

                    var searchResults = await _packageCache.GetSearchResults(searchQuery, refreshInstalled);

                    foreach (var entry in searchResults)
                    {
                        _dispatcherQueue.TryEnqueue(() => PackagesView.Add(new WingetPackageViewModel(entry)));
                    }
                    _dispatcherQueue.TryEnqueue(() => IsLoading = false);
                }
            }
        }

        private async Task InstallPackagesAsync(IEnumerable<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            foreach (var id in packageIds)
            {
                var installresult = await _packageManager.InstallPacakge(id, OnPackageInstallProgress);
            }

            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            await SerchPackageAsync(SearchQuery, true);
        }

        private async Task FetchPackageDetailsAsync(WingetPackageViewModel value)
        {
            if (_packages.Any(p => p.IsSelected))
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

        private void ViewPackageDetails(PackageDetailsViewModel details)
        {
            _navigationService.Navigate(NavigationItemKey.PackageDetails, new PackageDetailsNavigationArgs
            {
                PackageDetails = details,
                AvailableOperation = AvailableOperation.Install
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
            OnPropertyChanged(nameof(CanInstallSelected));
            OnPropertyChanged(nameof(CanInstallAll));
        }

        private void OnPackagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WingetPackageViewModel.IsSelected):
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(CanInstallSelected));
                    _ = FetchPackageDetailsAsync(SelectedPackage);
                    break;
                default:
                    break;
            }
        }
    }
}
