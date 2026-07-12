using System;

namespace WingetGUIInstaller.Services
{
    /// <summary>
    /// VeloPack implementation of IUpdateResponse.
    /// Holds update information from VeloPack UpdateManager.
    /// </summary>
    public sealed class VeloPackUpdateResponse : IUpdateResponse
    {
        public Version UpdateVersion { get; set; }

        public string ChangeLog { get; set; } = string.Empty;

        public bool IsUpdateAvailable { get; set; }

        public Uri UpdateUri { get; set; }

        public VeloPackUpdateResponse()
        {
        }

        public VeloPackUpdateResponse(Version updateVersion, string changeLog, Uri updateUri = null)
        {
            UpdateVersion = updateVersion;
            ChangeLog = changeLog ?? string.Empty;
            UpdateUri = updateUri;
            IsUpdateAvailable = updateVersion != null;
        }
    }
}
