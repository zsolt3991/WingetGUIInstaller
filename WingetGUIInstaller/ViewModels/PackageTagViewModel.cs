using CommunityToolkit.Mvvm.ComponentModel;

namespace WingetGUIInstaller.ViewModels
{
    public partial class PackageTagViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _tagName;

        public PackageTagViewModel(string tag)
        {
            _tagName = tag;
        }
    }
}
