using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetHelper.Commands;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class SearchPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ConsoleOutputCache _cache;
        private readonly ToastNotificationManager _notificationManager;
        private ObservableCollection<WingetPackageViewModel> _packages;
        private bool _isLoading;
        private WingetPackageViewModel _selectedPackage;
        private string _searchQuery;
        private string _loadingText;

        public SearchPageViewModel(DispatcherQueue dispatcherQueue, ConsoleOutputCache cache, ToastNotificationManager notificationManager)
        {
            _dispatcherQueue = dispatcherQueue;
            _cache = cache;
            _packages = new ObservableCollection<WingetPackageViewModel>();
            _notificationManager = notificationManager;
            Packages.CollectionChanged += Packages_CollectionChanged;
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

        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        public WingetPackageViewModel SelectedPackage
        {
            get => _selectedPackage;
            set
            {
                if (SetProperty(ref _selectedPackage, value))
                {
                    OnPropertyChanged(nameof(SelectedCount));
                }
            }
        }

        public int SelectedCount => Packages.Any(p => p.IsSelected) ? Packages.Count(p => p.IsSelected) : SelectedPackage != default ? 1 : 0;

        public bool CanInstallAll => Packages.Any();

        public bool CanInstallSelected => SelectedCount > 0;

        public ICommand SearchCommand => new AsyncRelayCommand(()
            => SerchPackageAsync(SearchQuery));

        public ICommand InstallSelectedCommand => new AsyncRelayCommand(()
            => InstallPackagesAsync(Packages.Where(p => p.IsSelected).Select(p => p.Id).ToList()));

        public ICommand InstalAllCommand => new AsyncRelayCommand(()
            => InstallPackagesAsync(Packages.Select(p => p.Id).ToList()));

        private async Task SerchPackageAsync(string searchQuery)
        {
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        Packages.Clear();
                        LoadingText = "Loading";
                        IsLoading = true;
                    });

                    var installedPackages = await PackageCommands.GetInstalledPackages()
                        .ConfigureOutputListener(_cache.IngestMessage)
                        .ExecuteAsync();
                    var searchResults = await PackageCommands.SearchPackages(searchQuery)
                        .ConfigureOutputListener(_cache.IngestMessage)
                        .ExecuteAsync();
                    foreach (var entry in searchResults)
                    {
                        if (!string.IsNullOrEmpty(entry.Source) && !installedPackages.Any(p => p.Id == entry.Id))
                        {
                            _dispatcherQueue.TryEnqueue(() => Packages.Add(new WingetPackageViewModel(entry)));
                        }
                    }
                    _dispatcherQueue.TryEnqueue(() => IsLoading = false);
                }
            }
        }

        private async Task InstallPackagesAsync(List<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            var successfulInstalls = 0;

            foreach (var id in packageIds)
            {
                var upgradeResult = await PackageCommands.InstallPackage(id)
                   .ConfigureProgressListener(OnPackageInstallProgress)
                   .ConfigureOutputListener(_cache.IngestMessage)
                   .ExecuteAsync();

                if (upgradeResult)
                {
                    successfulInstalls++;
                }

                if (packageIds.Count == 1)
                {
                    _notificationManager.ShowPackageOperationStatus(
                        Packages.FirstOrDefault(p => p.Id == id).Name, InstallOperation.Install, upgradeResult);
                }
            }

            if (packageIds.Count != 1)
            {
                _notificationManager.ShowMultiplePackageOperationStatus(
                    InstallOperation.Install, successfulInstalls, packageIds.Count - successfulInstalls);
            }

            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            await SerchPackageAsync(SearchQuery);
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
                    break;
                default:
                    break;
            }
        }
    }
}
