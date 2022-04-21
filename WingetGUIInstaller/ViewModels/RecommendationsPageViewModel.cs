using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class RecommendationsPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly PackageCache _packageCache;
        private readonly PackageManager _packageManager;
        private ObservableCollection<RecommendedItemsGroup> _recommendedItems;
        private string _loadingText;
        private bool _isLoading;
        private IEnumerable<RecommendedItemsGroup> _recommedationsList;

        public RecommendationsPageViewModel(DispatcherQueue dispatcherQueue,
            PackageCache packageCache, PackageManager packageManager)
        {
            _dispatcherQueue = dispatcherQueue;
            _packageCache = packageCache;
            _packageManager = packageManager;
            RecommendedItems = new ObservableCollection<RecommendedItemsGroup>();
            RecommendedItems.CollectionChanged += Packages_CollectionChanged;
            _ = LoadRecommendedItemsAsync(true);
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

        public ObservableCollection<RecommendedItemsGroup> RecommendedItems
        {
            get => _recommendedItems;
            set => SetProperty(ref _recommendedItems, value);
        }

        public int SelectedCount => RecommendedItems.Sum(group => group.Count(p => p.IsSelected));

        public bool CanInstallSelected => SelectedCount > 0;

        public bool CanInstallAll => RecommendedItems.Any(group => group.Any(p => !p.IsInstalled));

        public ICommand InstallSelectedCommand => new AsyncRelayCommand(()
          => InstallPackagesAsync(RecommendedItems.SelectMany(group => group.Where(p => p.IsSelected).Select(p => p.Id))));

        public ICommand InstalAllCommand => new AsyncRelayCommand(()
            => InstallPackagesAsync(RecommendedItems.SelectMany(group => group.Where(p => !p.IsInstalled).Select(p => p.Id))));

        public ICommand InstallGroupCommand => new AsyncRelayCommand<RecommendedItemsGroup>((group)
            => InstallPackagesAsync(group.Where(p => !p.IsInstalled).Select(p => p.Id)));

        private async Task LoadRecommendedItemsAsync(bool forceRefresh = false)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                LoadingText = "Loading";
                IsLoading = true;
            });

            var recommendations = JsonSerializer.Deserialize<List<RecommendedItem>>(
               File.ReadAllText(Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "recommended.json")),
              new JsonSerializerOptions
              {
                  Converters =
                  {
                        new JsonStringEnumConverter()
                  }
              });

            var installedPackages = await _packageCache.GetInstalledPackages(forceRefresh);

            _recommedationsList = recommendations.Select(r => new RecommendedItemViewModel(r)
            {
                IsInstalled = installedPackages.Any(p => p.Id == r.Id),
            }).GroupBy(r => r.Group, (key, values) => new RecommendedItemsGroup(key, values
                .OrderBy(r => r.Name).OrderBy(r => r.IsInstalled))).OrderBy(g => g.Key);

            _dispatcherQueue.TryEnqueue(() =>
            {
                UpdateDisplayedPackages();
                IsLoading = false;
            });
        }

        private async Task InstallPackagesAsync(IEnumerable<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            foreach (var id in packageIds)
            {
                var installResult = await _packageManager.InstallPacakge(id, OnPackageInstallProgress);
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);

            await LoadRecommendedItemsAsync(true);
        }

        private void UpdateDisplayedPackages()
        {
            RecommendedItems.Clear();
            foreach (var group in _recommedationsList)
            {
                RecommendedItems.Add(group);
            }
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
                    foreach (var packageEntry in (item as RecommendedItemsGroup).AsEnumerable())
                    {
                        packageEntry.PropertyChanged += OnPackagePropertyChanged;
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in e.OldItems)
                {
                    foreach (var packageEntry in (item as RecommendedItemsGroup).AsEnumerable())
                    {
                        packageEntry.PropertyChanged -= OnPackagePropertyChanged;
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
                case nameof(RecommendedItemViewModel.IsSelected):
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(CanInstallSelected));
                    break;
                default:
                    break;
            }
        }
    }
}
