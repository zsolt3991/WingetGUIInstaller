using CommunityToolkit.Mvvm.ComponentModel;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public partial class WingetPackageViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _id;

        [ObservableProperty]
        private string _version;

        [ObservableProperty]
        private string _available;

        [ObservableProperty]
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
    }
}
