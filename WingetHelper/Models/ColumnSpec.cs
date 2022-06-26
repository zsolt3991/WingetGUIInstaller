namespace WingetHelper.Models
{
    internal struct ColumnSpec
    {
        public string Name { get; set; }
        public int Length { get; set; }
        public bool IsLastColumn { get; set; }
    }
}
