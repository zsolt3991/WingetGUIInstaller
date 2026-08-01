using CommunityToolkit.Mvvm.Messaging.Messages;
using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Messages
{
    internal sealed class DefaultSortColumnChangedMessage : ValueChangedMessage<PackageSortColumn>
    {
        public DefaultSortColumnChangedMessage(PackageSortColumn value) : base(value)
        {
        }
    }
}
