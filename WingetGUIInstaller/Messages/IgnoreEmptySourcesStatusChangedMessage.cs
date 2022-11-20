using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WingetGUIInstaller.Messages
{
    internal sealed class IgnoreEmptySourcesStatusChangedMessage : ValueChangedMessage<bool>
    {
        public IgnoreEmptySourcesStatusChangedMessage(bool value) : base(value)
        {
        }
    }
}
