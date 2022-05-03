using Microsoft.UI.Xaml.Data;
using System;
using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Utils
{
    internal class NavigationItemKeyTagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is NavigationItemKey)
            {
                return value.ToString();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(NavigationItemKey))
            {
                var valueAsString = value.ToString();
                if (!string.IsNullOrEmpty(valueAsString))
                {
                    return Enum.Parse(typeof(NavigationItemKey), valueAsString);
                }
                else
                {
                    throw new ArgumentNullException(nameof(value));
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
