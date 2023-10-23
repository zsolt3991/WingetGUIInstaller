using CommunityToolkit.WinUI.Converters;
using Microsoft.UI.Xaml;

namespace WingetGUIInstaller.Utils
{
    internal sealed class EmptyObjectToVisibilityConverter : EmptyObjectToObjectConverter
    {
        public EmptyObjectToVisibilityConverter()
        {
            EmptyValue = Visibility.Collapsed;
            NotEmptyValue = Visibility.Visible;
        }
    }
}
