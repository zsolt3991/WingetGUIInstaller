using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WingetGUIInstaller.Messages
{
    internal sealed class FilterSourcesStatusChangedMessage : ValueChangedMessage<bool>
    {
        public FilterSourcesStatusChangedMessage(bool value) : base(value)
        {
        }
    }
}
