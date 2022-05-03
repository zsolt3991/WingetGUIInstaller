using CommunityToolkit.Mvvm.Messaging.Messages;
using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Models
{
    internal class NavigationRequestedMessage : ValueChangedMessage<NavigationItemKey>
    {
        public NavigationRequestedMessage(NavigationItemKey value) : base(value)
        {
        }
    }
}
