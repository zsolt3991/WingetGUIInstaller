using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [PageKey(Enums.NavigationItemKey.Upgrades)]
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
