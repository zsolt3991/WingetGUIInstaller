using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [NavigationKey(Enums.NavigationItemKey.PackageSources)]
    public sealed partial class PackageSourceManagementPage : Page
    {
        private readonly Dictionary<string, SortDirection?> _sortDirections;

        public PackageSourcePageViewModel ViewModel { get; }

        public PackageSourceManagementPage()
        {
            InitializeComponent();
            DataContext = ViewModel = Ioc.Default.GetRequiredService<PackageSourcePageViewModel>();
            _sortDirections = new Dictionary<string, SortDirection?>
            {
                {nameof(WingetPackageSourceViewModel.Name), null },
                {nameof(WingetPackageSourceViewModel.Argument), null }
            };
        }

        private async void AddSource_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await AddPackageSourceDialog.ShowAsync().AsTask();
        }

        private void NameColumnHeader_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            CollectionSorting(nameof(WingetPackageSourceViewModel.Name));
        }

        private void ArgumentColumnHeader_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            CollectionSorting(nameof(WingetPackageSourceViewModel.Argument));
        }

        private void CollectionSorting(string fieldName)
        {
            foreach (var sortingField in _sortDirections.Keys)
            {
                if (sortingField != fieldName)
                {
                    _sortDirections[sortingField] = null;
                }
            }

            _sortDirections[fieldName] = ViewModel.PackageSourcesView.ApplySorting(fieldName, _sortDirections[fieldName]);
        }
    }
}
