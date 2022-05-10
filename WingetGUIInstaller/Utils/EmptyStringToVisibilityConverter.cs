using CommunityToolkit.WinUI.UI.Converters;
using Microsoft.UI.Xaml;

namespace WingetGUIInstaller.Utils
{
    internal class EmptyStringToVisibilityConverter : EmptyStringToObjectConverter
    {
        public EmptyStringToVisibilityConverter()
        {
            EmptyValue = Visibility.Collapsed;
            NotEmptyValue = Visibility.Visible;
        }
    }
}