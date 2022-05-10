using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [PageKey(Enums.NavigationItemKey.Recommendations)]
    public sealed partial class RecommendationsPage : Page
    {
        public RecommendationsPageViewModel ViewModel { get; }

        public RecommendationsPage()
        {
            DataContext = ViewModel = Ioc.Default.GetRequiredService<RecommendationsPageViewModel>();
            InitializeComponent();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is RecommendedItemViewModel recommendedItem)
            {
                Frame.Navigate(typeof(PackageDetailsPage), new PackageDetailsNavigationArgs
                {
                    AvailableOperation = recommendedItem.IsInstalled ? AvailableOperation.Uninstall : AvailableOperation.Install,
                    PackageId = recommendedItem.Id,
                });
            }
        }
    }
}
