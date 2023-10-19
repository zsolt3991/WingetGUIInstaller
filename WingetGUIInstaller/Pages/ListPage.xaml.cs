using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [NavigationKey(Enums.NavigationItemKey.InstalledPackages)]
    public sealed partial class ListPage : Page
    {
        private readonly Dictionary<string, SortDirection?> _sortDirections;
        public ListPageViewModel ViewModel { get; }

        public ListPage()
        {
            InitializeComponent();
            DataContext = ViewModel = Ioc.Default.GetRequiredService<ListPageViewModel>();
            _sortDirections = new Dictionary<string, SortDirection?>
            {
                {nameof(WingetPackageViewModel.Name), null },
                {nameof(WingetPackageViewModel.Source), null }
            };
        }

        private void NameColumnHeader_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            CollectionSorting(nameof(WingetPackageViewModel.Name));
        }

        private void SourceColumnHeader_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            CollectionSorting(nameof(WingetPackageViewModel.Source));
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

            _sortDirections[fieldName] = ViewModel.PackagesView.ApplySorting(fieldName, _sortDirections[fieldName]);
        }
    }
}
