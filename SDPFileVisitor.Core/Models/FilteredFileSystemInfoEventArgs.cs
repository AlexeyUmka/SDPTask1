using System;

namespace SDPFileVisitor.Core.Models
{
    public class FilteredFileSystemInfoEventArgs : EventArgs
    {
        public string Name { get; set; }
        public bool Exclude { get; set; }
        public bool StopSearch { get; set; }
        public FilteredFileSystemInfoEventArgs(string name)
        {
            Name = name;
        }
    }
}
