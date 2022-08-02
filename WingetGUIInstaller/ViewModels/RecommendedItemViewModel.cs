using CommunityToolkit.Mvvm.ComponentModel;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class RecommendedItemViewModel : ObservableObject
    {
        private string _name;
        private string _id;
        private RecommendationGroupType _group;
        private bool _isInstalled;
        private bool _isSelected;
        private bool _hasUpdate;

        public RecommendedItemViewModel(RecommendedItem recommendedItem)
        {
            _name = recommendedItem.Name;
            _id = recommendedItem.Id;
            _group = recommendedItem.Group;
            _isInstalled = false;
            _isSelected = false;
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public RecommendationGroupType Group
        {
            get => _group;
            set => SetProperty(ref _group, value);
        }

        public bool IsInstalled
        {
            get => _isInstalled;
            set => SetProperty(ref _isInstalled, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool HasUpdate
        {
            get => _hasUpdate;
            set => SetProperty(ref _hasUpdate, value);
        }
    }
}
