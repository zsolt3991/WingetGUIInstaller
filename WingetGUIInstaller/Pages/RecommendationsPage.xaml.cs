using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
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
    }
}
