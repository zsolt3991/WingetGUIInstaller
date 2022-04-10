using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    public sealed partial class ConsolePage : Page
    {
        public ConsolePage()
        {
            DataContext = ViewModel = Ioc.Default.GetRequiredService<ConsolePageViewModel>();
            InitializeComponent();
        }

        public ConsolePageViewModel ViewModel { get; }
    }
}
