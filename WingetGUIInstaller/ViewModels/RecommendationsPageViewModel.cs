using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Navigation;
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
using Windows.ApplicationModel.Resources;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetHelper.Enums;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class RecommendationsPageViewModel : ObservableObject, INavigationAware
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly PackageCache _packageCache;
        private readonly PackageManager _packageManager;
        private readonly ToastNotificationManager _notificationManager;
        private readonly IReadOnlyList<RecommendedItem> _recommendedItemList;
        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForViewIndependentUse();

        [ObservableProperty]
        private string _loadingText;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ObservableCollection<RecommendedItemsGroup> _recommendedItems;

        public RecommendationsPageViewModel(DispatcherQueue dispatcherQueue,
            PackageCache packageCache, PackageManager packageManager, ToastNotificationManager notificationManager)
        {
            _dispatcherQueue = dispatcherQueue;
            _packageCache = packageCache;
            _packageManager = packageManager;
            _notificationManager = notificationManager;
            _recommendedItemList = LoadRecommendationsFile();
            RecommendedItems = new ObservableCollection<RecommendedItemsGroup>();
            RecommendedItems.CollectionChanged += Packages_CollectionChanged;
            _ = LoadRecommendedItemsAsync(true);
        }

        public int SelectedCount => RecommendedItems.Sum(group => group.Count(p => p.IsSelected));

        public bool CanInstallSelected => SelectedCount > 0;

        public bool CanInstallAll => RecommendedItems.Any(group => group.Any(p => !p.IsInstalled));

        [RelayCommand(CanExecute = nameof(CanInstallAll))]
        private async Task InstallAllPackagesAsync()
        {
            await InstallPackagesAsync(RecommendedItems.SelectMany(group => group.Where(p => !p.IsInstalled).Select(p => p.Id)));
        }

        [RelayCommand(CanExecute = nameof(CanInstallSelected))]
        private async Task InstallSelectedPackagesAsync()
        {
            await InstallPackagesAsync(RecommendedItems.SelectMany(group => group.Where(p => p.IsSelected).Select(p => p.Id)));
        }

        private async Task LoadRecommendedItemsAsync(bool forceRefresh = false)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                LoadingText = _resourceLoader.GetString("LoadingText");
                IsLoading = true;
            });

            if (_recommendedItemList != default && _recommendedItemList.Count > 0)
            {
                var installedPackages = await _packageCache.GetInstalledPackages(forceRefresh);
                var recommedationsList = _recommendedItemList
                    .Select(r =>
                    {
                        var isInstalled = installedPackages.Exists(p => p.Id == r.Id);
                        return new RecommendedItemViewModel(r)
                        {
                            IsInstalled = isInstalled,
                            HasUpdate = isInstalled && !string.IsNullOrEmpty(installedPackages.Find(p => p.Id == r.Id)?.Available)
                        };
                    })
                    .GroupBy(r => r.Group, (key, values) =>
                        new RecommendedItemsGroup(key, values
                        .OrderBy(r => r.Name)
                        .ThenBy(r => r.IsInstalled)))
                    .OrderBy(g => g.Key);

                _dispatcherQueue.TryEnqueue(() =>
                {
                    RecommendedItems.Clear();
                    foreach (var group in recommedationsList)
                    {
                        RecommendedItems.Add(group);
                    }
                });
            }

            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = false;
            });
        }

        private async Task InstallPackagesAsync(IEnumerable<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            WeakReferenceMessenger.Default.Send(new TopLevelNavigationAllowedMessage(false));

            var successfulInstalls = 0;
            foreach (var id in packageIds)
            {
                var installResult = await _packageManager.InstallPacakge(id, OnPackageInstallProgress);
                if (installResult)
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
            await LoadRecommendedItemsAsync(true);
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
                    if (item is RecommendedItemsGroup recommendedItems)
                    {
                        foreach (var packageEntry in recommendedItems)
                        {
                            packageEntry.PropertyChanged += OnPackagePropertyChanged;
                        }
                    }
                }
            }

            if ((e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace) && e.OldItems != default)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is RecommendedItemsGroup recommendedItems)
                    {
                        foreach (var packageEntry in recommendedItems)
                        {
                            packageEntry.PropertyChanged -= OnPackagePropertyChanged;
                        }
                    }
                }
            }

            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(CanInstallSelected));
            OnPropertyChanged(nameof(CanInstallAll));
            InstallSelectedPackagesCommand.NotifyCanExecuteChanged();
            InstallAllPackagesCommand.NotifyCanExecuteChanged();
        }

        private IReadOnlyList<RecommendedItem> LoadRecommendationsFile()
        {
            return JsonSerializer.Deserialize<List<RecommendedItem>>(
                File.ReadAllText(Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory) ?? string.Empty, "recommended.json")),
                new JsonSerializerOptions
                {
                    Converters =
                    {
                        new JsonStringEnumConverter()
                    }
                });
        }

        private void OnPackagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(RecommendedItemViewModel.IsSelected):
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(CanInstallSelected));
                    InstallSelectedPackagesCommand.NotifyCanExecuteChanged();
                    break;
                default:
                    break;
            }
        }

        public void OnNavigatedTo(object parameter)
        {
            _ = LoadRecommendedItemsAsync();
        }

        public void OnNavigatedFrom(NavigationMode navigationMode)
        {
        }
    }
}
