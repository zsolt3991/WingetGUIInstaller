using CommunityToolkit.Mvvm.Messaging.Messages;
using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Messages
{
    internal class NavigationRequestedMessage : ValueChangedMessage<NavigationItemKey>
    {
        public NavigationRequestedMessage(NavigationItemKey value) : base(value)
        {
        }
    }
}
