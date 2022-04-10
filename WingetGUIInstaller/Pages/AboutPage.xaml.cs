using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.ViewModels;


namespace WingetGUIInstaller.Pages
{
    public sealed partial class AboutPage : Page
    {
        public ApplicationInfoViewModel ViewModel { get; }

        public AboutPage()
        {
            DataContext = ViewModel = Ioc.Default.GetRequiredService<ApplicationInfoViewModel>();
            InitializeComponent();
        }
    }
}
