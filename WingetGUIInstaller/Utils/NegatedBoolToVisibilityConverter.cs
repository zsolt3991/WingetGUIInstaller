using CommunityToolkit.WinUI.UI.Converters;
using Microsoft.UI.Xaml;

namespace WingetGUIInstaller.Utils
{
    public class NegatedBoolToVisibilityConverter : BoolToObjectConverter
    {
        public NegatedBoolToVisibilityConverter()
        {
            TrueValue = Visibility.Collapsed;
            FalseValue = Visibility.Visible;
        }
    }
}
