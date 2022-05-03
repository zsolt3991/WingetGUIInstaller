using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WingetGUIInstaller.Messages
{
    internal class ConsoleEnabledChangeMessage : ValueChangedMessage<bool>
    {
        public ConsoleEnabledChangeMessage(bool value) : base(value)
        {
        }
    }
}
