using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WingetGUIInstaller.Messages
{
    internal sealed class FilterSourcesListUpdatedMessage : ValueChangedMessage<bool>
    {
        public FilterSourcesListUpdatedMessage(bool value) : base(value)
        {
        }
    }
}
