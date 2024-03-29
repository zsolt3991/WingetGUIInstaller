﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.ViewModels
{

    public sealed partial class RecommendedItemsGroup : ObservableObject, IGrouping<RecommendationGroupType, RecommendedItemViewModel>
    {
        private readonly IEnumerable<RecommendedItemViewModel> _items;
        private readonly RecommendationGroupType _key;

        [ObservableProperty]
        private bool _isSelected;

        public RecommendedItemsGroup(RecommendationGroupType key, IEnumerable<RecommendedItemViewModel> items)
        {
            _key = key;
            _items = items;
            foreach (var item in items)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        public RecommendationGroupType Key => _key;

        public bool CanSelect => _items.Any(p => !p.IsInstalled);

        public IEnumerator<RecommendedItemViewModel> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        partial void OnIsSelectedChanged(bool value)
        {
            foreach (var item in _items)
            {
                if (!item.IsInstalled)
                {
                    item.IsSelected = value;
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RecommendedItemViewModel.IsInstalled))
            {
                OnPropertyChanged(nameof(CanSelect));
            }
        }
    }
}
