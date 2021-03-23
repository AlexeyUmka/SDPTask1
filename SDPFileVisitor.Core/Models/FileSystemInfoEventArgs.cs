using System;

namespace SDPFileVisitor.Core.Models
{
    public class FileSystemInfoEventArgs : EventArgs
    {
        public string Name { get; set; }
        public bool Exclude { get; set; }
        public bool StopSearch { get; set; }

        public FileSystemInfoEventArgs(string name)
        {
            Name = name;
        }

    }
}
