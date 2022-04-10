using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPageViewModel ViewModel { get; }

        public SettingsPage()
        {
            DataContext = ViewModel = Ioc.Default.GetRequiredService<SettingsPageViewModel>();
            InitializeComponent();
        }

        private async void AddSource_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
           await AddPackageSourceDialog.ShowAsync().AsTask();
        }
    }
}
