using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [NavigationKey(Enums.NavigationItemKey.Search)]
    public sealed partial class SearchPage : Page
    {
        public SearchPageViewModel ViewModel { get; }

        public SearchPage()
        {
            InitializeComponent();
            DataContext = ViewModel = Ioc.Default.GetRequiredService<SearchPageViewModel>();
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            var propertyName = e.Column.Tag.ToString() switch
            {
                "Selected" => nameof(WingetPackageViewModel.IsSelected),
                "PackageId" => nameof(WingetPackageViewModel.Id),
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
