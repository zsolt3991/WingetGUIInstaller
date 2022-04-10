using CommunityToolkit.Mvvm.ComponentModel;

namespace WingetGUIInstaller.ViewModels
{
    public class WingetPackageSourceViewModel : ObservableObject
    {
        private bool _isSelected;
        private string _name;
        private string _url;
        private bool _isEnabled;

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

        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }
    }
}
