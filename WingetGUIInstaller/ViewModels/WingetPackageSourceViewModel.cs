using CommunityToolkit.Mvvm.ComponentModel;
using WingetHelper.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class WingetPackageSourceViewModel : ObservableObject
    {
        private bool _isSelected;
        private string _name;
        private string _argument;
        private bool _isEnabled;

        public WingetPackageSourceViewModel(WingetPackageSource packageSource)
        {
            _name = packageSource.Name;
            _argument = packageSource.Argument;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Argument
        {
            get => _argument;
            set => SetProperty(ref _argument, value);
        }
    }
}
