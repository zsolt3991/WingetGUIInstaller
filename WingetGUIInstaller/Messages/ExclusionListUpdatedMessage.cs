using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WingetGUIInstaller.Messages
{
    internal sealed class ExclusionListUpdatedMessage : ValueChangedMessage<bool>
    {
        public ExclusionListUpdatedMessage(bool exclusionAdded) : base(exclusionAdded)
        {
        }
    }
}
