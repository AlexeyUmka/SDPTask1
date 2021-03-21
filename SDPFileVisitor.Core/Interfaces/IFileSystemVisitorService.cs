using System;
using System.Collections.Generic;
using System.IO;
using SDPFileVisitor.Core.Models;

namespace SDPFileVisitor.Core.Interfaces
{
    public delegate void FileSystemHandler(object sender, FileSystemVisitorEventArgs e);
    public interface IFileSystemVisitorService
    {
        event FileSystemHandler SearchStarted;
        event FileSystemHandler SearchFinished;

        event FileSystemHandler DirectoryFound;
        event FileSystemHandler FileFound;

        event FileSystemHandler DirectoryFiltered;
        event FileSystemHandler FileFiltered;

        IEnumerable<FileSystemInfo> Search();
    }
}
