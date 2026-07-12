using CommunityToolkit.Mvvm.Messaging.Messages;
using WingetGUIInstaller.Contracts;

namespace WingetGUIInstaller.Messages
{
    internal sealed class UpdateAvailableMessage : ValueChangedMessage<IUpdateResponse>
    {
        /// <summary>
        /// Creates message with unified IUpdateResponse interface.
        /// Works with both GitHub (via adapter) and VeloPack responses.
        /// </summary>
        public UpdateAvailableMessage(IUpdateResponse value) : base(value)
        {
        }
    }
}
