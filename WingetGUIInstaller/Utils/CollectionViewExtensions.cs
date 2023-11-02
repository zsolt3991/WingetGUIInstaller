using CommunityToolkit.WinUI.Collections;
using System;

namespace WingetGUIInstaller.Utils
{
    public static class CollectionViewExtensions
    {
        public static SortDirection? ApplySorting(this AdvancedCollectionView advancedCollectionView,
            string propertyName, SortDirection? currentSorting)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                if (currentSorting == null || currentSorting == SortDirection.Descending)
                {
                    advancedCollectionView.SortDescriptions.Clear();
                    advancedCollectionView.SortDescriptions.Add(new SortDescription(propertyName, SortDirection.Ascending));
                    return SortDirection.Ascending;
                }
                else
                {
                    advancedCollectionView.SortDescriptions.Clear();
                    advancedCollectionView.SortDescriptions.Add(new SortDescription(propertyName, SortDirection.Descending));
                    return SortDirection.Descending;
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
                    advancedCollectionView.Filter = p => filterExpression(p as TElement);
                }
                catch
                {
                    // Ignore exceptions during filter as this runs on UI thread
                }
            }
        }

        public static void ClearFiltering(this AdvancedCollectionView advancedCollectionView)
        {
            using (advancedCollectionView.DeferRefresh())
            {
                try
                {
                    advancedCollectionView.Filter = p => true;
                }
                catch
                {
                    // Ignore exceptions during filter as this runs on UI thread
                }
            }
        }
    }
}
