using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [PageKey(Enums.NavigationItemKey.Upgrades)]
    public sealed partial class UpgradePage : Page
    {
        public UpgradePageViewModel ViewModel { get; }
        public UpgradePage()
        {
            DataContext = ViewModel = Ioc.Default.GetRequiredService<UpgradePageViewModel>();
            InitializeComponent();
        }

        public ICommand GoToDetailsCommand =>
            new RelayCommand<PackageDetailsViewModel>(ViewPackageDetails);

        private void ViewPackageDetails(PackageDetailsViewModel obj)
        {
            Frame.Navigate(typeof(PackageDetailsPage), new PackageDetailsNavigationArgs
            {
                PackageDetails = obj,
                AvailableOperation = AvailableOperation.Uninstall | AvailableOperation.Update
            });
        }
    }
}
