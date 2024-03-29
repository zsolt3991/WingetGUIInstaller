﻿using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [NavigationKey(Enums.NavigationItemKey.ImportExport)]
    public sealed partial class ImportExportPage : Page
    {
        public ImportExportPage()
        {
            InitializeComponent();
            DataContext = ViewModel = Ioc.Default.GetRequiredService<ImportExportPageViewModel>();
        }

        public ImportExportPageViewModel ViewModel { get; }
    }
}
