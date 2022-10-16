using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.UI.Xaml;

namespace WingetGUIInstaller.Messages
{
    internal sealed class ThemeChangedMessage : ValueChangedMessage<ElementTheme>
    {
        public ThemeChangedMessage(ElementTheme value) : base(value)
        {
        }
    }
}
