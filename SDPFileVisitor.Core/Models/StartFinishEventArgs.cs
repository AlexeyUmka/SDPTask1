using System.Diagnostics;

namespace SDPFileVisitor.Core.Models
{
    public class StartFinishEventArgs
    {
        public Stopwatch Stopwatch { get; set; }
        public bool StopSearch { get; set; }
    }
}
