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
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetHelper.Enums;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class SearchPageViewModel : ObservableObject,
        IRecipient<FilterSourcesListUpdatedMessage>,
        IRecipient<FilterSourcesStatusChangedMessage>,
        IRecipient<IgnoreEmptySourcesStatusChangedMessage>,
        INavigationAware
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ToastNotificationManager _notificationManager;
        private readonly PackageCache _packageCache;
        private readonly PackageManager _packageManager;
        private readonly INavigationService<NavigationItemKey> _navigationService;
        private readonly PackageDetailsCache _packageDetailsCache;
        private readonly ObservableCollection<WingetPackageViewModel> _packages;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SearchQueryValid))]
        [NotifyCanExecuteChangedFor(nameof(SearchPackagesCommand))]
        private string _searchQuery;

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
        [NotifyPropertyChangedFor(nameof(CanInstallSelected))]
        [NotifyCanExecuteChangedFor(nameof(InstallSelectedPackagesCommand))]
        private WingetPackageViewModel _selectedPackage;

        public SearchPageViewModel(DispatcherQueue dispatcherQueue, ToastNotificationManager notificationManager,
            PackageCache packageCache, PackageManager packageManager, INavigationService<NavigationItemKey> navigationService,
            PackageDetailsCache packageDetailsCache)
        {
            _dispatcherQueue = dispatcherQueue;
            _packageCache = packageCache;
            _packageManager = packageManager;
            _notificationManager = notificationManager;
            _navigationService = navigationService;
            _packageDetailsCache = packageDetailsCache;
            _packages = new ObservableCollection<WingetPackageViewModel>();
            _notificationManager = notificationManager;
            _packages.CollectionChanged += Packages_CollectionChanged;
            PackagesView = new AdvancedCollectionView(_packages, true);
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public int SelectedCount => _packages.Any(p => p.IsSelected) ? _packages.Count(p => p.IsSelected) : SelectedPackage != default ? 1 : 0;

        public bool CanInstallAll => _packages.Any();

        public bool CanInstallSelected => SelectedCount > 0;

        public bool SearchQueryValid => !string.IsNullOrWhiteSpace(SearchQuery);

        [RelayCommand(CanExecute = nameof(DetailsAvailable))]
        private void ViewPackageDetails(string packageId)
        {
            _navigationService.Navigate(NavigationItemKey.PackageDetails, args: new PackageDetailsNavigationArgs
            {
                PackageId = packageId,
                AvailableOperation = AvailableOperation.Install
            });
        }

        [RelayCommand(CanExecute = nameof(CanInstallSelected))]
        private async Task InstallSelectedPackages()
        {
            await InstallPackagesAsync(GetSelectedPackageIds());
        }

        [RelayCommand(CanExecute = nameof(CanInstallAll))]
        private async Task InstallAllPackages()
        {
            await InstallPackagesAsync(_packages.Select(p => p.Id));
        }

        [RelayCommand(CanExecute = nameof(SearchQueryValid))]
        private async Task SearchPackagesAsync()
        {
            await SerchPackagesAsync(SearchQuery, false);
        }

        partial void OnSelectedPackageChanged(WingetPackageViewModel value)
        {
            _ = FetchPackageDetailsAsync(value);
        }

        private async Task SerchPackagesAsync(string searchQuery, bool refreshInstalled = false)
        {
            if (SearchQueryValid)
            {
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        _packages.Clear();
                        LoadingText = "Loading";
                        IsLoading = true;
                    });

                    var searchResults = await _packageCache.GetSearchResults(searchQuery, refreshInstalled);

                    foreach (var entry in searchResults)
                    {
                        _dispatcherQueue.TryEnqueue(() => _packages.Add(new WingetPackageViewModel(entry)));
                    }
                    _dispatcherQueue.TryEnqueue(() => IsLoading = false);
                }
            }
        }

        private async Task InstallPackagesAsync(IEnumerable<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(false));

            var successfulInstalls = 0;
            foreach (var id in packageIds)
            {

                var installresult = await _packageManager.InstallPacakge(id, OnPackageInstallProgress);
                if (installresult)
                {
                    successfulInstalls++;
                }
            }

            if (packageIds.Any())
            {
                _notificationManager.ShowBatchPackageOperationStatus(
                    InstallOperation.Install, packageIds.Count(), successfulInstalls);
            }

            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(true));
            await SerchPackagesAsync(SearchQuery, true);
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
            OnPropertyChanged(nameof(CanInstallSelected));
            OnPropertyChanged(nameof(CanInstallAll));
            InstallSelectedPackagesCommand.NotifyCanExecuteChanged();
            InstallAllPackagesCommand.NotifyCanExecuteChanged();
        }

        private void OnPackagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WingetPackageViewModel.IsSelected):
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(CanInstallSelected));
                    InstallSelectedPackagesCommand.NotifyCanExecuteChanged();
                    InstallAllPackagesCommand.NotifyCanExecuteChanged();
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
            _ = SearchPackagesAsync();
        }

        void IRecipient<FilterSourcesStatusChangedMessage>.Receive(FilterSourcesStatusChangedMessage message)
        {
            _ = SearchPackagesAsync();
        }

        void IRecipient<FilterSourcesListUpdatedMessage>.Receive(FilterSourcesListUpdatedMessage message)
        {
            _ = SearchPackagesAsync();
        }

        public void OnNavigatedTo(object parameter)
        {
            _ = SearchPackagesAsync();
        }

        public void OnNavigatedFrom(NavigationMode navigationMode)
        {
        }
    }
}
