using CommunityToolkit.Mvvm.ComponentModel;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class WingetPackageViewModel : ObservableObject
    {
        private bool _isSelected;
        private string _name;
        private string _id;
        private string _version;
        private string _available;
        private string _source;

        public WingetPackageViewModel(WingetPackageEntry packageEntry)
        {
            _available = packageEntry.Available;
            _id = packageEntry.Id;
            _name = packageEntry.Name;
            _source = packageEntry.Source;
            _version = packageEntry.Version;
            _isSelected = false;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        public string Available
        {
            get => _available;
            set => SetProperty(ref _available, value);
        }

        public string Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }
    }
}
