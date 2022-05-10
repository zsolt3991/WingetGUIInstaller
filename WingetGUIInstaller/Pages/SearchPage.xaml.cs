using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [PageKey(Enums.NavigationItemKey.Search)]
    public sealed partial class SearchPage : Page
    {
        public SearchPageViewModel ViewModel { get; }

        public SearchPage()
        {
            DataContext = ViewModel = Ioc.Default.GetRequiredService<SearchPageViewModel>();
            InitializeComponent();
        }
    }
}
