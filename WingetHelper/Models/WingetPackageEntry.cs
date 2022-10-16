namespace WingetHelper.Models
{
    public sealed class WingetPackageEntry
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public string Available { get; set; }
        public string Source { get; set; }
    }
}