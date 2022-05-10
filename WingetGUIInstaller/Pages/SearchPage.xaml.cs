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
