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
using WingetGUIInstaller.Services;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class ListPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly PackageCache _packageCache;
        private readonly PackageManager _packageManager;
        private ObservableCollection<WingetPackageViewModel> _packages;
        private bool _isLoading;
        private WingetPackageViewModel _selectedPackage;
        private string _filterText;
        private IEnumerable<WingetPackageEntry> _returnedPackages;
        private string _loadingText;
        private PackageDetailsViewModel _selectedPackageDetails;
        private bool _isDetailsLoading;

        public ListPageViewModel(DispatcherQueue dispatcherQueue,
            PackageCache packageCache, PackageManager packageManager)
        {
            _dispatcherQueue = dispatcherQueue;
            _packageCache = packageCache;
            _packageManager = packageManager;
            _packages = new ObservableCollection<WingetPackageViewModel>();
            Packages.CollectionChanged += Packages_CollectionChanged;
            _ = FetchInstalledPackages();
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
                    UpdateDisplayedPackages();
                }
            }
        }

        public int SelectedCount
            => Packages.Any(p => p.IsSelected) ? Packages.Count(p => p.IsSelected) : SelectedPackage != default ? 1 : 0;

        public ICommand ListCommand => new AsyncRelayCommand(RefreshInstalledPackages);

        public ICommand UpgradeSelectedCommand => new AsyncRelayCommand(() =>
             UpgradePackages(Packages.Where(p => p.IsSelected).Select(p => p.Id)));

        public ICommand UninstallSelectedCommand => new AsyncRelayCommand(() =>
            UninstallPackages(Packages.Where(p => p.IsSelected).Select(p => p.Id)));

        public bool IsSomethingSelected => SelectedCount > 0;

        public bool DetailsAvailable
        {
            get => _isDetailsLoading;
            private set => SetProperty(ref _isDetailsLoading, value);
        }

        private async Task FetchInstalledPackages() => await LoadInstalledPackages();

        private async Task RefreshInstalledPackages() => await LoadInstalledPackages(true);

        private async Task LoadInstalledPackages(bool forceUpdate = false)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = true;
                LoadingText = "Loading";
            });

            _returnedPackages = await _packageCache.GetInstalledPackages(forceUpdate);

            _dispatcherQueue.TryEnqueue(() =>
            {
                UpdateDisplayedPackages();
                IsLoading = false;
            });
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

        private async Task UpgradePackages(IEnumerable<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            foreach (var id in packageIds)
            {
                var upgradeResult = await _packageManager.UpgradePackage(id, OnPackageInstallProgress);
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            await RefreshInstalledPackages();
        }

        private async Task UninstallPackages(IEnumerable<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            foreach (var id in packageIds)
            {
                var uninstallResult = await _packageManager.RemovePackage(id, OnPackageInstallProgress);
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            await RefreshInstalledPackages();
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
    }
}
