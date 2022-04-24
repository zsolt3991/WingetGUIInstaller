using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WingetGUIInstaller.Models
{
    internal class NavigationRequestedMessage : ValueChangedMessage<NavigationItem>
    {
        public NavigationRequestedMessage(NavigationItem value) : base(value)
        {
        }
    }
}
