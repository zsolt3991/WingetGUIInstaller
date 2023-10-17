using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;

namespace WingetGUIInstaller.ViewModels
{
    public partial class PackageTagViewModel : ObservableObject
    {
        private readonly INavigationService<NavigationItemKey> _navigationService;

        [ObservableProperty]
        private string _tagName;

        public PackageTagViewModel(string tag, INavigationService<NavigationItemKey> navigationService)
        {
            _tagName = tag;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void GoToTagSearch()
        {
            _navigationService.Navigate(NavigationItemKey.Search, args: new SearchArguments(TagName));
        }
    }
}
