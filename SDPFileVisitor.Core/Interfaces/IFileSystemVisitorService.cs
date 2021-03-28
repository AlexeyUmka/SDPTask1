using System;
using System.Collections.Generic;
using System.IO;
using SDPFileVisitor.Core.Models;

namespace SDPFileVisitor.Core.Interfaces
{
    public interface IFileSystemVisitorService
    {
        event EventHandler<StartFinishEventArgs> SearchStarted;
        event EventHandler<StartFinishEventArgs> SearchFinished;

        event EventHandler<FileSystemInfoEventArgs> DirectoryFound;
        event EventHandler<FileSystemInfoEventArgs> FileFound;

        event EventHandler<FilteredFileSystemInfoEventArgs> DirectoryFiltered;
        event EventHandler<FilteredFileSystemInfoEventArgs> FileFiltered;

        IEnumerable<FileSystemInfoModel> Search();
    }
}
