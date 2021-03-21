using System;
using System.Collections.Generic;
using System.IO;
using SDPFileVisitor.Core.Interfaces;
using SDPFileVisitor.Core.Models;

namespace SDPFileVisitor.Core.Services
{
    public class FileSystemVisitorService : IFileSystemVisitorService
    {
        private readonly string _startPath;
        private readonly Predicate<FileSystemInfo> _matchPredicate;

        public event FileSystemHandler SearchStarted;
        public event FileSystemHandler SearchFinished;
        public event FileSystemHandler DirectoryFound;
        public event FileSystemHandler FileFound;
        public event FileSystemHandler DirectoryFiltered;
        public event FileSystemHandler FileFiltered;


        public FileSystemVisitorService(
            string startPath,
            Predicate<FileSystemInfo> matchPredicate)
        {
            _startPath = startPath;
            _matchPredicate = matchPredicate;
        }

        public IEnumerable<FileSystemInfo> Search()
        {
            var eventArgs = new FileSystemVisitorEventArgs();
            SearchStarted?.Invoke(this, eventArgs);
            var rootDirectory = new DirectoryInfo(_startPath);
            foreach (var fileSystemInfo in rootDirectory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                if (eventArgs.StopSearch)
                {
                    SearchFinished?.Invoke(this, eventArgs);
                    yield break;
                }

                if (fileSystemInfo != null)
                {
                    eventArgs.Name = fileSystemInfo.FullName;
                    if (fileSystemInfo is FileInfo)
                    {
                        FileFound?.Invoke(this, eventArgs);
                        if (_matchPredicate(fileSystemInfo) && !eventArgs.ExcludePredicate(fileSystemInfo))
                        {
                            FileFiltered?.Invoke(this, eventArgs);
                            yield return fileSystemInfo;
                        }
                    }
                    else if (fileSystemInfo is DirectoryInfo)
                    {
                        DirectoryFound?.Invoke(this, eventArgs);
                        if (_matchPredicate(fileSystemInfo) && !eventArgs.ExcludePredicate(fileSystemInfo))
                        {
                            DirectoryFiltered?.Invoke(this, eventArgs);
                            yield return fileSystemInfo;
                        }
                    }
                }
            }

            SearchFinished?.Invoke(this, eventArgs);
        }
    }
}
