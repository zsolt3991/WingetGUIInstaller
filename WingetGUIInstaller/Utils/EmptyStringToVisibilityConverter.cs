using CommunityToolkit.WinUI.Converters;
using Microsoft.UI.Xaml;

namespace WingetGUIInstaller.Utils
{
    internal sealed class EmptyStringToVisibilityConverter : EmptyStringToObjectConverter
    {
        public EmptyStringToVisibilityConverter()
        {
            EmptyValue = Visibility.Collapsed;
            NotEmptyValue = Visibility.Visible;
        }
    }
}