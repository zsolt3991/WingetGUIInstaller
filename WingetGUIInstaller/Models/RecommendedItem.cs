using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Models
{
    public class RecommendedItem
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public RecommendationGroupType Group { get; set; }
    }
}
