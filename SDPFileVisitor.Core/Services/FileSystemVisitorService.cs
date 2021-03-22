﻿using System;
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

        public event FileSystemHandler<StartFinishEventArgs> SearchFinished;
        public event FileSystemHandler<StartFinishEventArgs> SearchStarted;

        public event FileSystemHandler<FileSystemInfoEventArgs> DirectoryFound;
        public event FileSystemHandler<FileSystemInfoEventArgs> FileFound;

        public event FileSystemHandler<FilteredFileSystemInfoEventArgs> DirectoryFiltered;
        public event FileSystemHandler<FilteredFileSystemInfoEventArgs> FileFiltered;

        public FileSystemVisitorService(string startPath)
        {
            _startPath = startPath;
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
            var startFinishEventArgs = new StartFinishEventArgs();
            SearchStarted?.Invoke(this, startFinishEventArgs);
            if (startFinishEventArgs.StopSearch)
            {
                SearchFinished?.Invoke(this, startFinishEventArgs);
                return new List<FileSystemInfo>();
            }
            var result = GetFileSystemInfo(new DirectoryInfo(_startPath)).ToList();
            SearchFinished?.Invoke(this, startFinishEventArgs);
            return result;
        }

        IEnumerable<FileSystemInfo> GetFileSystemInfo(DirectoryInfo directoryInfo)
        {
            var allElements = directoryInfo.GetFileSystemInfos();
            foreach (var fileSystemInfo in allElements)
            {
                var itemFoundEventArgs = new FileSystemInfoEventArgs() { Name = fileSystemInfo.FullName};
                var itemFilteredEventArgs = new FilteredFileSystemInfoEventArgs() { Name = fileSystemInfo.FullName};
                if (fileSystemInfo is FileInfo)
                {
                    FileFound?.Invoke(this, itemFoundEventArgs);
                    if (itemFoundEventArgs.StopSearch)
                    {
                        yield break;
                    }
                    if (_matchPredicate(fileSystemInfo) && !itemFoundEventArgs.Exclude)
                    {
                        FileFiltered?.Invoke(this, itemFilteredEventArgs);
                        if (itemFilteredEventArgs.StopSearch)
                        {
                            yield break;
                        }
                        if (!itemFilteredEventArgs.Exclude)
                        {
                            yield return fileSystemInfo;
                        }
                    }
                }
                else if (fileSystemInfo is DirectoryInfo nextDirectory)
                {
                    DirectoryFound?.Invoke(this, itemFoundEventArgs);
                    if (itemFoundEventArgs.StopSearch)
                    {
                        yield break;
                    }
                    if (_matchPredicate(fileSystemInfo) && !itemFoundEventArgs.Exclude)
                    {
                        foreach (var nextFileSystemInfo in GetFileSystemInfo(nextDirectory))
                        {
                            yield return nextFileSystemInfo;
                        }
                        DirectoryFiltered?.Invoke(this, itemFilteredEventArgs);
                        if (itemFilteredEventArgs.StopSearch)
                        {
                            yield break;
                        }
                        if (!itemFilteredEventArgs.Exclude)
                        {
                            yield return fileSystemInfo;
                        }
                    }
                }
            }
        }
    }
}
