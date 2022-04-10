using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WingetGUIInstaller.Models
{
    internal class ConsoleEnabledChangeMessage : ValueChangedMessage<bool>
    {
        public ConsoleEnabledChangeMessage(bool value) : base(value)
        {
        }
    }
}
