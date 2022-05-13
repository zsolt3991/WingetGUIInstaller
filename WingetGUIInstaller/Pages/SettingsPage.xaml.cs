﻿using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using System;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [PageKey(Enums.NavigationItemKey.Settings)]
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

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            var propertyName = e.Column.Tag.ToString() switch
            {
                "SourceSelected" => nameof(WingetPackageSourceViewModel.IsSelected),
                "SourceName" => nameof(WingetPackageSourceViewModel.Name),
                "SourceUrl" => nameof(WingetPackageSourceViewModel.Url),
                "SourceEnabled" => nameof(WingetPackageSourceViewModel.IsEnabled),
                _ => string.Empty
            };

            foreach (var dgColumn in PackageSourcesGrid.Columns)
            {
                if (dgColumn.Tag.ToString() != e.Column.Tag.ToString())
                {
                    dgColumn.SortDirection = null;
                }
            }

            e.Column.SortDirection = ViewModel.PackageSourcesView.ApplySorting(propertyName, e.Column.SortDirection);
        }
    }
}
