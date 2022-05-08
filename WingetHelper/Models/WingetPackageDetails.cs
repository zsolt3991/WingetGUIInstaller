using WingetHelper.Utils;

namespace WingetHelper.Models
{
    public class WingetPackageDetails
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }

        public string Version { get; set; }

        public string Publisher { get; set; }

        public string Moniker { get; set; }

        public string Description { get; set; }

        public string Homepage { get; set; }

        public string License { get; set; }

        [DeserializerName("License Url")]
        public string License_Url { get; set; }

        [DeserializerName("Privacy Url")]
        public string Privacy_Url { get; set; }

        public string Copyright { get; set; }

        [DeserializerName("Release Notes")]
        public string Release_Notes { get; set; }

        public WingetPackageInstaller Installer { get; set; }
    }

    public class WingetPackageInstaller
    {
        public string Type { get; set; }

        [DeserializerName("Download Url")]
        public string Download_Url { get; set; }

        public string SHA256 { get; set; }
    }
}
