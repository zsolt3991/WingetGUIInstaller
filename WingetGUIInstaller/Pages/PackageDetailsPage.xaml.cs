using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [PageKey(NavigationItemKey.PackageDetails)]
    public sealed partial class PackageDetailsPage : Page
    {
        private readonly INavigationService<NavigationItemKey> _navigationService;

        internal PackageDetailsPageViewModel ViewModel { get; }

        public PackageDetailsPage()
        {
            InitializeComponent();
            _navigationService = Ioc.Default.GetRequiredService<INavigationService<NavigationItemKey>>();
            DataContext = ViewModel = Ioc.Default.GetRequiredService<PackageDetailsPageViewModel>();
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PackageDetailsNavigationArgs packageDetails)
            {
                ViewModel.UpdateData(packageDetails);
            }

            base.OnNavigatedTo(e);
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.GoBack();
        }
    }
}
