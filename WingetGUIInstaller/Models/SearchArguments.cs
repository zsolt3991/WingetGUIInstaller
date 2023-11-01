namespace WingetGUIInstaller.Models
{
    internal sealed class SearchArguments
    {
        public string TagName { get; private set; }

        public SearchArguments(string tagName)
        {
            TagName = tagName;
        }
    }
}