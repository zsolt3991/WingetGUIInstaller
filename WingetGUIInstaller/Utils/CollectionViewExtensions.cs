using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using System;

namespace WingetGUIInstaller.Utils
{
    public static class CollectionViewExtensions
    {
        public static DataGridSortDirection? ApplySorting(this AdvancedCollectionView advancedCollectionView,
            string propertyName, DataGridSortDirection? currentSorting)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                if (currentSorting == null || currentSorting == DataGridSortDirection.Descending)
                {
                    advancedCollectionView.SortDescriptions.Clear();
                    advancedCollectionView.SortDescriptions.Add(new SortDescription(propertyName, SortDirection.Ascending));
                    return DataGridSortDirection.Ascending;
                }
                else
                {
                    advancedCollectionView.SortDescriptions.Clear();
                    advancedCollectionView.SortDescriptions.Add(new SortDescription(propertyName, SortDirection.Descending));
                    return DataGridSortDirection.Descending;
                }
            }
            return default;
        }

        public static void ApplyFiltering<TElement>(this AdvancedCollectionView advancedCollectionView,
            Predicate<TElement> filterExpression) where TElement : class
        {
            using (advancedCollectionView.DeferRefresh())
            {
                try
                {
                    advancedCollectionView.Filter = p => filterExpression.Invoke(p as TElement);
                }
                catch { }
            }
        }

        public static void ClearFiltering(this AdvancedCollectionView advancedCollectionView)
        {
            using (advancedCollectionView.DeferRefresh())
            {
                try
                {
                    advancedCollectionView.Filter = default;
                }
                catch { }
            }
        }
    }
}
