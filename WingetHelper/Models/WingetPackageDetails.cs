using YamlDotNet.Serialization;

namespace WingetHelper.Models
{
    public class WingetPackageDetails
    {
        public string Author { get; set; }

        public string Version { get; set; }

        public string Publisher { get; set; }

        public string Moniker { get; set; }

        public string Description { get; set; }

        public string Homepage { get; set; }

        public string License { get; set; }

        [YamlMember(Alias = "License Url", ApplyNamingConventions = false)]
        public string License_Url { get; set; }

        [YamlMember(Alias = "Privacy Url", ApplyNamingConventions = false)]
        public string Privacy_Url { get; set; }

        public string Copyright { get; set; }

        [YamlMember(Alias = "Release Notes", ApplyNamingConventions = false)]
        public string Release_Notes { get; set; }

        public WingetPackageInstaller Installer { get; set; }
    }

    public class WingetPackageInstaller
    {
        public string Type { get; set; }

        [YamlMember(Alias = "Download Url", ApplyNamingConventions = false)]
        public string Download_Url { get; set; }

        public string SHA256 { get; set; }
    }
}
