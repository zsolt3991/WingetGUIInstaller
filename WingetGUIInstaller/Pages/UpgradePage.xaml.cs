using CommunityToolkit.Mvvm.DependencyInjection;
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
            var propertyName = e.Column.Tag.ToString() switch
            {
                "Selected" => nameof(WingetPackageViewModel.IsSelected),
                "PackageName" => nameof(WingetPackageViewModel.Name),
                "Source" => nameof(WingetPackageViewModel.Source),
                _ => string.Empty
            };

            foreach (var dgColumn in PackagesGrid.Columns)
            {
                if (dgColumn.Tag.ToString() != e.Column.Tag.ToString())
                {
                    dgColumn.SortDirection = null;
                }
            }

            e.Column.SortDirection = ViewModel.PackagesView.ApplySorting(propertyName, e.Column.SortDirection);
        }
    }
}
