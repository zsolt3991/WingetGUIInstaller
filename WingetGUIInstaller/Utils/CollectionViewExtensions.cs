using CommunityToolkit.Common.Extensions;
using CommunityToolkit.Helpers;
using CommunityToolkit.WinUI.Collections;
using System;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Enums;

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

        /// <summary>
        /// Applies the default ascending sort based on the user's configured sort column preference.
        /// Should be called once at ViewModel construction; user interactive sorts override this for the session.
        /// </summary>
        public static void ApplyDefaultPackageSort(this AdvancedCollectionView advancedCollectionView,
            ISettingsStorageHelper<string> configurationStore)
        {
            var column = (PackageSortColumn)configurationStore
                .GetValueOrDefault(ConfigurationPropertyKeys.DefaultPackageSortColumn, ConfigurationPropertyKeys.DefaultPackageSortColumnDefaultValue);
            // PackageSortColumn enum values (Name, Id, Source) intentionally match WingetPackageViewModel property names
            advancedCollectionView.ApplySorting(column.ToString(), null);
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
