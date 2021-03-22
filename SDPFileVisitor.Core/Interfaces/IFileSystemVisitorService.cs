using System;
using System.Collections.Generic;
using System.IO;
using SDPFileVisitor.Core.Models;

namespace SDPFileVisitor.Core.Interfaces
{
    public delegate void FileSystemHandler<TEventArgs>(object sender, TEventArgs e);
    public interface IFileSystemVisitorService
    {
        event FileSystemHandler<StartFinishEventArgs> SearchStarted;
        event FileSystemHandler<StartFinishEventArgs> SearchFinished;

        event FileSystemHandler<FileSystemInfoEventArgs> DirectoryFound;
        event FileSystemHandler<FileSystemInfoEventArgs> FileFound;

        event FileSystemHandler<FilteredFileSystemInfoEventArgs> DirectoryFiltered;
        event FileSystemHandler<FilteredFileSystemInfoEventArgs> FileFiltered;

        IEnumerable<FileSystemInfo> Search();
    }
}
