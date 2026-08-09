using System.Runtime.Serialization;

namespace WingetGUIInstaller.Enums
{
    public enum RecommendationGroupType
    {
        [EnumMember(Value = "Basic Utilities")]
        BasicUtilities,
        Development,
        Productivity,
        Gaming,
        Graphics,
        [EnumMember(Value = "AI Tools")]
        AiTools
    }
}
