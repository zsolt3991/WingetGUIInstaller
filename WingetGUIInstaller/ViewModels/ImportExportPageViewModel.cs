using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;

namespace WingetGUIInstaller.ViewModels
{
    public class ImportExportPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly PackageManager _packageManager;
        private readonly PackageSourceCache _packageSourceCache;
        private StorageFile _importFile;
        private StorageFile _exportFile;
        private bool _isLoading;
        private string _loadingText;
        private IReadOnlyList<string> _packageSources;
        private string _selectedSourceName;
        private bool _exportVersions;
        private bool _importVersions;
        private bool _ignoreMissing;

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

        public StorageFile ImportFile
        {
            get => _importFile;
            set => SetProperty(ref _importFile, value);
        }

        public StorageFile ExportFile
        {
            get => _exportFile;
            set => SetProperty(ref _exportFile, value);
        }

        public IReadOnlyList<string> PackageSources
        {
            get => _packageSources;
            set => SetProperty(ref _packageSources, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool ExportVersions
        {
            get => _exportVersions;
            set => SetProperty(ref _exportVersions, value);
        }

        public bool ImportVersions
        {
            get => _importVersions;
            set => SetProperty(ref _importVersions, value);
        }

        public bool IgnoreMissing
        {
            get => _ignoreMissing;
            set => SetProperty(ref _ignoreMissing, value);
        }

        public string LoadingText
        {
            get => _loadingText;
            set => SetProperty(ref _loadingText, value);
        }

        public string SelectedSourceName
        {
            get => _selectedSourceName;
            set => SetProperty(ref _selectedSourceName, value);
        }

        public string ImportFileText => ImportFile != default ? ImportFile.Path : "No file selected";

        public bool CanImport => ImportFile != default;

        public ICommand BrowseImportFileCommand => new AsyncRelayCommand(BrowseImportFileAsync);

        public ICommand ImportPackageListCommand => new AsyncRelayCommand(ImportPackageListAsync);

        public string ExportFileText => ExportFile != default ? ExportFile.Path : "No file selected";

        public bool CanExport => ExportFile != default;

        public ICommand BrowseExportFileCommand => new AsyncRelayCommand(BrowseExportFileAsync);

        public ICommand ExportPackageListCommand => new AsyncRelayCommand(ExportPackageListAsync);

        private async Task ExportPackageListAsync()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                LoadingText = "Exporting";
                IsLoading = true;
            });

            await _packageManager.ExportPackageList(ExportFile, ExportVersions,
                SelectedSourceName != "All sources" ? SelectedSourceName : default);
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
        }

        private async Task ImportPackageListAsync()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                LoadingText = "Importing";
                IsLoading = true;
            });

            await _packageManager.ImportPackageList(ImportFile, ImportVersions, IgnoreMissing);
            _dispatcherQueue.TryEnqueue(() => IsLoading = false);
        }

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
            OnPropertyChanged(nameof(ExportFileText));
            OnPropertyChanged(nameof(CanExport));
        }

        private async Task BrowseImportFileAsync()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
            };
            picker.FileTypeFilter.Add(".json");

            ImportFile = await picker.InitializeWithAppWindow().PickSingleFileAsync();
            OnPropertyChanged(nameof(ImportFileText));
            OnPropertyChanged(nameof(CanImport));
        }

        private async Task LoadPackageSourceListAsync()
        {
            var packageSources = await _packageSourceCache.GetAvailablePackageSources();
            PackageSources = packageSources.Select(source => source.Name)
                .Append("All sources")
                .OrderBy(source => source)
                .ToList();
            SelectedSourceName = PackageSources.FirstOrDefault();
        }
    }
}
