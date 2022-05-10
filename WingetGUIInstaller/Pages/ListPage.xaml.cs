using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    public sealed partial class ListPage : Page
    {
        public ListPageViewModel ViewModel { get; }

        public ListPage()
        {
            DataContext = ViewModel = Ioc.Default.GetRequiredService<ListPageViewModel>();
            InitializeComponent();
        }

        public ICommand GoToDetailsCommand =>
            new RelayCommand<PackageDetailsViewModel>(ViewPackageDetails);

        private void ViewPackageDetails(PackageDetailsViewModel obj)
        {
            Frame.Navigate(typeof(PackageDetailsPage), new PackageDetailsNavigationArgs
            {
                PackageDetails = obj,
                AvailableOperation = AvailableOperation.Uninstall
            });
        }
    }
}
