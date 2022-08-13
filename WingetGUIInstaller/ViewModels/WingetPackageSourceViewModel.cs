using CommunityToolkit.Mvvm.ComponentModel;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public partial class WingetPackageSourceViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _argument;

        [ObservableProperty]
        private bool _isEnabled;

        public WingetPackageSourceViewModel(WingetPackageSource packageSource, bool isEnabled = false)
        {
            _name = packageSource.Name;
            _argument = packageSource.Argument;
            _isEnabled = isEnabled;
        }
    }
}
