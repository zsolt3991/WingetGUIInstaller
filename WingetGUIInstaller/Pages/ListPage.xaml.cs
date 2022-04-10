using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
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
    }
}
