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
using WingetHelper.Commands;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class ListPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ConsoleOutputCache _cache;
        private ObservableCollection<WingetPackageViewModel> _packages;
        private bool _isLoading;
        private WingetPackageViewModel _selectedPackage;
        private string _filterText;
        private List<WingetPackageEntry> _returnedPackages;
        private string _loadingText;

        public ListPageViewModel(DispatcherQueue dispatcherQueue, ConsoleOutputCache cache)
        {
            _dispatcherQueue = dispatcherQueue;
            _cache = cache;
            _packages = new ObservableCollection<WingetPackageViewModel>();
            Packages.CollectionChanged += Packages_CollectionChanged;
            _ = ListInstalledPackages();
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

        public WingetPackageViewModel SelectedPackage
        {
            get => _selectedPackage;
            set
            {
                if (SetProperty(ref _selectedPackage, value))
                {
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(IsSomethingSelected));
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

        public int SelectedCount => Packages.Any(p => p.IsSelected) ? Packages.Count(p => p.IsSelected) : SelectedPackage != default ? 1 : 0;

        public ICommand ListCommand => new AsyncRelayCommand(ListInstalledPackages);

        public ICommand UpgradeSelectedCommand => new AsyncRelayCommand(() =>
             UpgradePackages(Packages.Where(p => p.IsSelected).Select(p => p.Id)));

        public ICommand UninstallSelectedCommand => new AsyncRelayCommand(() =>
            UninstallPackages(Packages.Where(p => p.IsSelected).Select(p => p.Id)));

        public bool IsSomethingSelected => SelectedCount > 0;

        private async Task ListInstalledPackages()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = true;
                LoadingText = "Loading";
            });

            _returnedPackages = (await PackageCommands.GetInstalledPackages()
                .ConfigureOutputListener(_cache.IngestMessage)
                .ExecuteAsync()).ToList();

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
                var upgradeResult = await PackageCommands.UpgradePackage(id)
                    .ConfigureOutputListener(_cache.IngestMessage)
                    .ConfigureProgressListener(OnPackageInstallProgress)
                    .ExecuteAsync();
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
           await ListInstalledPackages();
        }

        private async Task UninstallPackages(IEnumerable<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            foreach (var id in packageIds)
            {
                var upgradeResult = PackageCommands.UninstallPackage(id)
                    .ConfigureOutputListener(_cache.IngestMessage)
                    .ConfigureProgressListener(OnPackageInstallProgress)
                    .ExecuteAsync();
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            await ListInstalledPackages();
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
            OnPropertyChanged(nameof(IsSomethingSelected));
        }

        private void OnPackagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WingetPackageViewModel.IsSelected):
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(IsSomethingSelected));
                    break;
                default:
                    break;
            }
        }
    }
}
