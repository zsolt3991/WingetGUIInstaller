using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [NavigationKey(Enums.NavigationItemKey.ExcludedPackages)]
    public sealed partial class ExcludedPackagesPage : Page
    {
        public ExcludedPackagesPage()
        {
            InitializeComponent();
            DataContext = ViewModel = Ioc.Default.GetRequiredService<ExcludedPackagesViewModel>();
        }

        public ExcludedPackagesViewModel ViewModel { get; }

        private async void AddExclusion_ClickAsync(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await PackageSelectorDialog.ShowAsync();
        }
    }
}
