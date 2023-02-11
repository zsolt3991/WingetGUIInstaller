using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WingetGUIInstaller.Messages
{
    internal sealed class TopLevelNavigationAllowedMessage : ValueChangedMessage<bool>
    {
        public TopLevelNavigationAllowedMessage(bool value) : base(value)
        {
        }
    }
}
