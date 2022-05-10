using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [PageKey(NavigationItemKey.Recommendations)]
    public sealed partial class RecommendationsPage : Page
    {
        private readonly INavigationService<NavigationItemKey> _navigationService;

        public RecommendationsPageViewModel ViewModel { get; }

        public RecommendationsPage()
        {
            _navigationService = Ioc.Default.GetRequiredService<INavigationService<NavigationItemKey>>();
            DataContext = ViewModel = Ioc.Default.GetRequiredService<RecommendationsPageViewModel>();
            InitializeComponent();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is RecommendedItemViewModel recommendedItem)
            {
                _navigationService.Navigate(NavigationItemKey.PackageDetails, new PackageDetailsNavigationArgs
                {
                    AvailableOperation = recommendedItem.IsInstalled ? AvailableOperation.Uninstall : AvailableOperation.Install,
                    PackageId = recommendedItem.Id,
                });
            }
        }
    }
}
