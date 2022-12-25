using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [NavigationKey(NavigationItemKey.PackageDetails)]
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
    }
}
