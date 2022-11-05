using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WingetGUIInstaller.Messages
{
    internal class ExclusionStatusChangedMessage : ValueChangedMessage<bool>
    {
        public ExclusionStatusChangedMessage(bool value) : base(value)
        {
        }
    }
}
