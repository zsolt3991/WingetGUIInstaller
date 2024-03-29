﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class ImportExportPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly PackageManager _packageManager;
        private readonly PackageSourceCache _packageSourceCache;
        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForViewIndependentUse();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _loadingText;

        [ObservableProperty]
        private string _selectedSourceName;

        [ObservableProperty]
        private bool _exportVersions;

        [ObservableProperty]
        private bool _importVersions;

        [ObservableProperty]
        private bool _ignoreMissing;

        [ObservableProperty]
        private IReadOnlyList<string> _packageSources;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ImportFileText))]
        [NotifyPropertyChangedFor(nameof(CanImport))]
        [NotifyCanExecuteChangedFor(nameof(ImportPackageListCommand))]
        private StorageFile _importFile;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ExportFileText))]
        [NotifyPropertyChangedFor(nameof(CanExport))]
        [NotifyCanExecuteChangedFor(nameof(ExportPackageListCommand))]
        private StorageFile _exportFile;

        public ImportExportPageViewModel(DispatcherQueue dispatcherQueue, PackageManager packageManager,
            PackageSourceCache packageSourceCache)
        {
            _dispatcherQueue = dispatcherQueue;
            _packageManager = packageManager;
            _packageSourceCache = packageSourceCache;

            IgnoreMissing = true;
            ImportVersions = false;
            ExportVersions = false;

            _ = LoadPackageSourceListAsync();
        }

        public string ImportFileText => ImportFile != default ? ImportFile.Path : _resourceLoader.GetString("NoFileSelectedText");

        public bool CanImport => ImportFile != default;

        public string ExportFileText => ExportFile != default ? ExportFile.Path : _resourceLoader.GetString("NoFileSelectedText");

        public bool CanExport => ExportFile != default;

        [RelayCommand(CanExecute = nameof(CanExport))]
        private async Task ExportPackageListAsync()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                LoadingText = _resourceLoader.GetString("ExportingText");
                IsLoading = true;
            });

            await _packageManager.ExportPackageList(ExportFile, ExportVersions,
                SelectedSourceName != _resourceLoader.GetString("AllSourcesText") ? SelectedSourceName : default);
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
        }

        [RelayCommand(CanExecute = nameof(CanImport))]
        private async Task ImportPackageListAsync()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                LoadingText = _resourceLoader.GetString("ImportingText");
                IsLoading = true;
            });

            await _packageManager.ImportPackageList(ImportFile, ImportVersions, IgnoreMissing);
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
        }

        [RelayCommand]
        private async Task BrowseExportFileAsync()
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
                DefaultFileExtension = ".json",
                SuggestedFileName = "export"
            };
            picker.FileTypeChoices.Add("JSON file", new List<string> { ".json" });

            ExportFile = await picker.InitializeWithAppWindow().PickSaveFileAsync();
        }

        [RelayCommand]
        private async Task BrowseImportFileAsync()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
            };
            picker.FileTypeFilter.Add(".json");

            ImportFile = await picker.InitializeWithAppWindow().PickSingleFileAsync();
        }

        private async Task LoadPackageSourceListAsync()
        {
            var packageSources = await _packageSourceCache.GetAvailablePackageSources();
            PackageSources = packageSources.Select(source => source.Name)
                .Append(_resourceLoader.GetString("AllSourcesText"))
                .OrderBy(source => source)
                .ToList();
            SelectedSourceName = PackageSources[0];
        }
    }
}
