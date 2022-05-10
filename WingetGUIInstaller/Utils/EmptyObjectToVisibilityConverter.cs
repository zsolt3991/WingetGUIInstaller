using CommunityToolkit.WinUI.UI.Converters;
using Microsoft.UI.Xaml;

namespace WingetGUIInstaller.Utils
{
    internal class EmptyObjectToVisibilityConverter : EmptyObjectToObjectConverter
    {
        public EmptyObjectToVisibilityConverter()
        {
            EmptyValue = Visibility.Collapsed;
            NotEmptyValue = Visibility.Visible;
        }
    }
}
