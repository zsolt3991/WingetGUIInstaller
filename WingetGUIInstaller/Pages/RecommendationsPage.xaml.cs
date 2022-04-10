using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
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
