using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [NavigationKey(Enums.NavigationItemKey.Settings)]
    public sealed partial class SettingsPage : Page
    {
        public SettingsPageViewModel ViewModel { get; }

        public SettingsPage()
        {
            InitializeComponent();
            DataContext = ViewModel = Ioc.Default.GetRequiredService<SettingsPageViewModel>();
        }      
    }
}
