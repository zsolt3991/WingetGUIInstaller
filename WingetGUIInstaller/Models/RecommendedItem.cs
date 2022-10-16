using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Models
{
    public sealed class RecommendedItem
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public RecommendationGroupType Group { get; set; }
    }
}
