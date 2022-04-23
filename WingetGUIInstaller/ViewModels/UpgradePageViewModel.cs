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
using WingetGUIInstaller.Services;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class UpgradePageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly PackageCache _packageCache;
        private readonly PackageManager _packageManager;
        private readonly ObservableCollection<WingetPackageViewModel> _packages;
        private bool _isLoading;
        private WingetPackageViewModel _selectedPackage;
        private string _filterText;
        private string _loadingText;
        private AdvancedCollectionView _packagesView;

        public UpgradePageViewModel(DispatcherQueue dispatcherQueue,
            PackageCache packageCache, PackageManager packageManager)
        {
            _dispatcherQueue = dispatcherQueue;
            _packageCache = packageCache;
            _packageManager = packageManager;
            _packages = new ObservableCollection<WingetPackageViewModel>();
            _packages.CollectionChanged += Packages_CollectionChanged;
            PackagesView = new AdvancedCollectionView(_packages, true);
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

        public AdvancedCollectionView PackagesView
        {
            get => _packagesView;
            set => SetProperty(ref _packagesView, value);
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

        public WingetPackageViewModel SelectedPackage
        {
            get => _selectedPackage;
            set
            {
                if (SetProperty(ref _selectedPackage, value))
                {
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(CanUpgradeSelected));
                }
            }
        }

        public int SelectedCount => _packages.Any(p => p.IsSelected) ?
            _packages.Count(p => p.IsSelected) : SelectedPackage != default ? 1 : 0;

        public bool CanUpgradeAll => _packages.Any();

        public bool CanUpgradeSelected => SelectedCount > 0;

        public ICommand RefreshCommand => new AsyncRelayCommand(()
            => ListUpgradableItemsAsync(true));

        public ICommand UpgradeSelectedCommand
            => new AsyncRelayCommand(() => UpgradePackagesAsync(_packages.Where(p => p.IsSelected).Select(p => p.Id)));

        public ICommand UpgradeAllCommand
            => new AsyncRelayCommand(() => UpgradePackagesAsync(_packages.Select(p => p.Id)));

        private async Task ListUpgradableItemsAsync(bool forceReload = false)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = true;
                LoadingText = "Loading";
            });

            var _returnedPackages = await _packageCache.GetUpgradablePackages(forceReload);

            _dispatcherQueue.TryEnqueue(() =>
            {
                UpdateDisplayedPackages(_returnedPackages);
                IsLoading = false;
            });
        }

        private void UpdateDisplayedPackages(IEnumerable<WingetPackageEntry> wingetPackages)
        {
            _packages.Clear();
            foreach (var entry in wingetPackages)
            {
                _packages.Add(new WingetPackageViewModel(entry));
            }
        }

        private async Task UpgradePackagesAsync(IEnumerable<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            foreach (var id in packageIds)
            {
                var upgradeResult = await _packageManager.UpgradePackage(id, OnPackageInstallProgress);
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            await ListUpgradableItemsAsync(true);
        }

        private void OnPackageInstallProgress(WingetProcessState progess)
        {
            _dispatcherQueue.TryEnqueue(() => LoadingText = progess.ToString());
        }

        private void Packages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    (item as WingetPackageViewModel).PropertyChanged += OnPackagePropertyChanged;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in e.OldItems)
                {
                    (item as WingetPackageViewModel).PropertyChanged -= OnPackagePropertyChanged;
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
                    break;
                default:
                    break;
            }
        }


        private void ApplyPackageFilter(string query)
        {
            using (PackagesView.DeferRefresh())
            {
                try
                {
                    PackagesView.Filter = p => IsMatchingFilter(p as WingetPackageViewModel, query);
                }
                catch { }
            }
        }

        private bool IsMatchingFilter(WingetPackageViewModel package, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            return package.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                || package.Id.Contains(query, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
