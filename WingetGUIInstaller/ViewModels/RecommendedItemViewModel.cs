using CommunityToolkit.Mvvm.ComponentModel;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;

namespace WingetGUIInstaller.ViewModels
{
    public partial class RecommendedItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _id;

        [ObservableProperty]
        private RecommendationGroupType _group;

        [ObservableProperty]
        private bool _isInstalled;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _hasUpdate;

        public RecommendedItemViewModel(RecommendedItem recommendedItem)
        {
            _name = recommendedItem.Name;
            _id = recommendedItem.Id;
            _group = recommendedItem.Group;
            _isInstalled = false;
            _isSelected = false;
        }
    }
}
