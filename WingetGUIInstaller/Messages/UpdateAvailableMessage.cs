using CommunityToolkit.Mvvm.Messaging.Messages;
using GithubPackageUpdater.Models;

namespace WingetGUIInstaller.Messages
{
    internal sealed class UpdateAvailableMessage : ValueChangedMessage<PackageUpdateResponse>
    {
        public UpdateAvailableMessage(PackageUpdateResponse value) : base(value)
        {
        }
    }
}
