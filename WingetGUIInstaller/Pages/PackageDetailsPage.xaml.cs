using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
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

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
