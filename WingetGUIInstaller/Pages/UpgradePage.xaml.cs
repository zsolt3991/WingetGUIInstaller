using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [NavigationKey(NavigationItemKey.Upgrades)]
    public sealed partial class UpgradePage : Page
    {
        private readonly Dictionary<string, SortDirection?> _sortDirections;

        public UpgradePageViewModel ViewModel { get; }

        public UpgradePage()
        {
            InitializeComponent();
            DataContext = ViewModel = Ioc.Default.GetRequiredService<UpgradePageViewModel>();
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
