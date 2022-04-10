namespace WingetHelper.Models
{
    internal class ColumnSpec
    {
        public string Name { get; set; }
        public int StartIndex { get; set; }
        public int MaxLength { get; set; }
        public bool IsLastColumn { get; set; }
    }
}
