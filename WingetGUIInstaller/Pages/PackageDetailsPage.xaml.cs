using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [NavigationKey(NavigationItemKey.PackageDetails)]
    public sealed partial class PackageDetailsPage : Page
    {
        internal PackageDetailsPageViewModel ViewModel { get; }

        public PackageDetailsPage()
        {
            InitializeComponent();
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
