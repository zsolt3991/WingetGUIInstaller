﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Notifications;
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
    public class UpgradePageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ConsoleOutputCache _cache;
        private ObservableCollection<WingetPackageViewModel> _packages;
        private bool _isLoading;
        private WingetPackageViewModel _selectedPackage;
        private string _filterText;
        private IEnumerable<WingetPackageEntry> _returnedPackages;
        private string _loadingText;

        public UpgradePageViewModel(DispatcherQueue dispatcherQueue, ConsoleOutputCache cache)
        {
            _dispatcherQueue = dispatcherQueue;
            _cache = cache;
            _packages = new ObservableCollection<WingetPackageViewModel>();
            Packages.CollectionChanged += Packages_CollectionChanged;
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

        public ObservableCollection<WingetPackageViewModel> Packages
        {
            get => _packages;
            set => SetProperty(ref _packages, value);
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

        public int SelectedCount => Packages.Any(p => p.IsSelected) ?
            Packages.Count(p => p.IsSelected) : SelectedPackage != default ? 1 : 0;

        public bool CanUpgradeAll => Packages.Any();

        public bool CanUpgradeSelected => SelectedCount > 0;

        public ICommand RefreshCommand => new AsyncRelayCommand(()
            => ListUpgradableItemsAsync());

        public ICommand UpgradeSelectedCommand => new AsyncRelayCommand(() =>
            UpgradePackagesAsync(Packages.Where(p => p.IsSelected).Select(p => p.Id)));

        public ICommand UpgradeAllCommand => new AsyncRelayCommand(() =>
            UpgradePackagesAsync(Packages.Select(p => p.Id)));

        private async Task ListUpgradableItemsAsync()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = true;
                LoadingText = "Loading";
            });
            _returnedPackages = await PackageCommands.GetUpgradablePackages()
                .ConfigureOutputListener(OnOutputEvent)
                .ExecuteAsync();

            new ToastContentBuilder()
                .AddText(string.Format("{0} update(s) available", _returnedPackages.Count()))
                .Show();

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

        private async Task UpgradePackagesAsync(IEnumerable<string> packageIds)
        {
            _dispatcherQueue.TryEnqueue(() => IsLoading = true);
            foreach (var id in packageIds)
            {
                var upgradeResult = await PackageCommands.UpgradePackage(id)
                   .ConfigureProgressListener(OnPackageInstallProgress)
                   .ConfigureOutputListener(OnOutputEvent)
                   .ExecuteAsync();
            }
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
            await ListUpgradableItemsAsync();
        }

        private void OnOutputEvent(string message)
        {
            _cache.IngestMessage(message);
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
    }
}
