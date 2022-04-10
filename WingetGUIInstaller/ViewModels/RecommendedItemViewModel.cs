using CommunityToolkit.Mvvm.ComponentModel;
using WingetGUIInstaller.Models;

namespace WingetGUIInstaller.ViewModels
{
    public class RecommendedItemViewModel : ObservableObject
    {
        private string _name;
        private string _id;
        private GroupType _group;
        private bool _isInstalled;
        private bool _isSelected;

        public RecommendedItemViewModel()
        {
        }

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

        public GroupType Group
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
    }
}
