using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    public sealed partial class UpgradePage : Page
    {
        public UpgradePageViewModel ViewModel { get; }
        public UpgradePage()
        {
            DataContext = ViewModel = Ioc.Default.GetRequiredService<UpgradePageViewModel>();
            InitializeComponent();
        }
    }
}
