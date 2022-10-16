using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WingetGUIInstaller.Messages
{
    internal sealed class CommandlineOutputMessage : ValueChangedMessage<string>
    {
        public CommandlineOutputMessage(string value) : base(value)
        {
        }
    }
}
