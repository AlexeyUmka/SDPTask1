namespace SDPFileVisitor.Core.Models
{
    public class FilteredFileSystemInfoEventArgs
    {
        public string Name { get; set; }
        public bool Exclude { get; set; } = false;
    }
}
