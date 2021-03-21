using System;
using System.Diagnostics;
using System.IO;

namespace SDPFileVisitor.Core.Models
{
    public class FileSystemVisitorEventArgs
    {
        public string Name { get; set; }
        public bool StopSearch { get; set; }
        public Predicate<FileSystemInfo> ExcludePredicate { get; set; } = (x) => false;
        public Stopwatch Stopwatch { get; set; }
    }
}
