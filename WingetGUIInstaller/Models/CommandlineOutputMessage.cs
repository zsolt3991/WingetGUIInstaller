using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WingetGUIInstaller.Models
{
    internal class CommandlineOutputMessage : ValueChangedMessage<string>
    {
        public CommandlineOutputMessage(string value) : base(value)
        {
        }
    }
}
