using CommunityToolkit.WinUI.Converters;
using Microsoft.UI.Xaml;

namespace WingetGUIInstaller.Utils
{
    public sealed class NegatedBoolToVisibilityConverter : BoolToObjectConverter
    {
        public NegatedBoolToVisibilityConverter()
        {
            TrueValue = Visibility.Collapsed;
            FalseValue = Visibility.Visible;
        }
    }
}
