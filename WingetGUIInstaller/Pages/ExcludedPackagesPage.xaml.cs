using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [PageKey(Enums.NavigationItemKey.ExcludedPackages)]
    public sealed partial class ExcludedPackagesPage : Page
    {
        public ExcludedPackagesPage()
        {
            InitializeComponent(); 
            DataContext = ViewModel = Ioc.Default.GetRequiredService<ExcludedPackagesViewModel>();
        }

        public ExcludedPackagesViewModel ViewModel { get; }
    }
}
