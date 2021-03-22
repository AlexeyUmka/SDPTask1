namespace SDPFileVisitor.Core.Models
{
    public class FileSystemInfoEventArgs
    {
        public string Name { get; set; }
        public bool Exclude { get; set; }

        public bool StopSearch { get; set; }

    }
}
