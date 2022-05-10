using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    public sealed partial class SearchPage : Page
    {
        public SearchPageViewModel ViewModel { get; }

        public SearchPage()
        {
            DataContext = ViewModel = Ioc.Default.GetRequiredService<SearchPageViewModel>();
            InitializeComponent();
        }

        public ICommand GoToDetailsCommand =>
            new RelayCommand<PackageDetailsViewModel>(ViewPackageDetails);

        private void ViewPackageDetails(PackageDetailsViewModel obj)
        {
            Frame.Navigate(typeof(PackageDetailsPage), new PackageDetailsNavigationArgs
            {
                PackageDetails = obj,
                AvailableOperation = AvailableOperation.Install
            });
        }
    }
}
