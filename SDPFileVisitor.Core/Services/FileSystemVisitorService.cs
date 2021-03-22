using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SDPFileVisitor.Core.Interfaces;
using SDPFileVisitor.Core.Models;

namespace SDPFileVisitor.Core.Services
{
    public class FileSystemVisitorService : IFileSystemVisitorService
    {
        private readonly string _startPath;
        private readonly Predicate<FileSystemInfo> _matchPredicate;
        private readonly StartFinishEventArgs _startFinishEventArgs = new StartFinishEventArgs();
        private readonly FileSystemInfoEventArgs _itemFoundEventArgs = new FileSystemInfoEventArgs();
        private readonly FilteredFileSystemInfoEventArgs _itemFilteredEventArgs = new FilteredFileSystemInfoEventArgs();

        public event FileSystemHandler<StartFinishEventArgs> SearchFinished;
        public event FileSystemHandler<StartFinishEventArgs> SearchStarted;

        public event FileSystemHandler<FileSystemInfoEventArgs> DirectoryFound;
        public event FileSystemHandler<FileSystemInfoEventArgs> FileFound;

        public event FileSystemHandler<FilteredFileSystemInfoEventArgs> DirectoryFiltered;
        public event FileSystemHandler<FilteredFileSystemInfoEventArgs> FileFiltered;

        public FileSystemVisitorService()
        {
            _startPath = "C:\\";
            _matchPredicate = (x) => true;
        }

        public FileSystemVisitorService(
            string startPath,
            Predicate<FileSystemInfo> matchPredicate)
        {
            _startPath = startPath;
            _matchPredicate = matchPredicate;
        }

        public IEnumerable<FileSystemInfo> Search()
        {
            SearchStarted?.Invoke(this, _startFinishEventArgs);
            var result = GetFileSystemInfo(new DirectoryInfo(_startPath)).ToList();
            SearchFinished?.Invoke(this, _startFinishEventArgs);
            return result;
        }

        IEnumerable<FileSystemInfo> GetFileSystemInfo(DirectoryInfo directoryInfo)
        {
            if (_startFinishEventArgs.StopSearch)
            {
                SearchFinished?.Invoke(this, _startFinishEventArgs);
                yield break;
            }

            var allElements = directoryInfo.GetFileSystemInfos("*");
            foreach (var fileSystemInfo in allElements)
            {
                _itemFoundEventArgs.Name = fileSystemInfo.FullName;
                _itemFilteredEventArgs.Name = fileSystemInfo.FullName;
                if (fileSystemInfo is FileInfo)
                {
                    FileFound?.Invoke(this, _itemFoundEventArgs);
                    if (_matchPredicate(fileSystemInfo) && !_itemFilteredEventArgs.Exclude)
                    {
                        FileFiltered?.Invoke(this, _itemFilteredEventArgs);
                        yield return fileSystemInfo;
                    }
                }
                else if (fileSystemInfo is DirectoryInfo nextDirectory)
                {
                    DirectoryFound?.Invoke(this, _itemFoundEventArgs);
                    if (_matchPredicate(fileSystemInfo) && !_itemFilteredEventArgs.Exclude)
                    {
                        foreach (var nextFileSystemInfo in GetFileSystemInfo(nextDirectory))
                        {
                            yield return nextFileSystemInfo;
                        }
                        _itemFilteredEventArgs.Name = fileSystemInfo.FullName;
                        DirectoryFiltered?.Invoke(this, _itemFilteredEventArgs);
                        yield return fileSystemInfo;
                    }
                }
            }
        }
    }
}
