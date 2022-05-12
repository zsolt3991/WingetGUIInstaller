﻿using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [PageKey(NavigationItemKey.Upgrades)]
    public sealed partial class UpgradePage : Page
    {
        public UpgradePageViewModel ViewModel { get; }

        public UpgradePage()
        {
            InitializeComponent();
            DataContext = ViewModel = Ioc.Default.GetRequiredService<UpgradePageViewModel>();
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            var propertyName = e.Column.Header.ToString() switch
            {
                "Package Name" => nameof(WingetPackageViewModel.Name),
                "Source" => nameof(WingetPackageViewModel.Source),
                _ => string.Empty
            };

            e.Column.SortDirection = ViewModel.PackagesView.ApplySorting(propertyName, e.Column.SortDirection);
        }
    }
}
