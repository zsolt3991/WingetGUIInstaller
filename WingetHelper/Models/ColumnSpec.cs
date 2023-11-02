namespace WingetHelper.Models
{
    internal readonly struct ColumnSpec
    {
        public ColumnSpec(string headerName, int dataLength, bool isLastColumn = false)
        {
            Name = headerName;
            MaxLength = dataLength;
            IsLastColumn = isLastColumn;
        }

        public string Name { get; }
        public int MaxLength { get; }
        public bool IsLastColumn { get; }
    }
}
