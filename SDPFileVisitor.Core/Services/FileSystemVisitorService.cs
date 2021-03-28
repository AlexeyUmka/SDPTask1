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
        private readonly Predicate<FileSystemInfoModel> _matchPredicate;
        private readonly IDirectoryInfoService _directoryInfoService;

        public event EventHandler<StartFinishEventArgs> SearchFinished;
        public event EventHandler<StartFinishEventArgs> SearchStarted;

        public event EventHandler<FileSystemInfoEventArgs> DirectoryFound;
        public event EventHandler<FileSystemInfoEventArgs> FileFound;

        public event EventHandler<FilteredFileSystemInfoEventArgs> DirectoryFiltered;
        public event EventHandler<FilteredFileSystemInfoEventArgs> FileFiltered;

        public FileSystemVisitorService(string startPath, IDirectoryInfoService directoryInfoService)
        {
            _startPath = startPath;
            _matchPredicate = (x) => true;
            _directoryInfoService = directoryInfoService;
        }

        public FileSystemVisitorService(
            string startPath,
            Predicate<FileSystemInfoModel> matchPredicate,
            IDirectoryInfoService directoryInfoService)
        {
            _startPath = startPath;
            _matchPredicate = matchPredicate;
            _directoryInfoService = directoryInfoService;
        }

        public IEnumerable<FileSystemInfoModel> Search()
        {
            var startFinishEventArgs = new StartFinishEventArgs();
            SearchStarted?.Invoke(this, startFinishEventArgs);
            if (startFinishEventArgs.StopSearch)
            {
                SearchFinished?.Invoke(this, startFinishEventArgs);
                return new List<FileSystemInfoModel>();
            }
            var result = GetFileSystemInfo(_startPath).ToList();
            SearchFinished?.Invoke(this, startFinishEventArgs);
            return result;
        }

        IEnumerable<FileSystemInfoModel> GetFileSystemInfo(string directoryInfo)
        {
            var allElements = _directoryInfoService.GetFileSystemInfos(directoryInfo);
            foreach (var fileSystemInfo in allElements)
            {
                var itemFoundEventArgs = new FileSystemInfoEventArgs(fileSystemInfo.FullName);
                var itemFilteredEventArgs = new FilteredFileSystemInfoEventArgs(fileSystemInfo.FullName);
                if (fileSystemInfo.SystemItemType == SystemItemType.File)
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
                else if (fileSystemInfo.SystemItemType == SystemItemType.Directory)
                {
                    DirectoryFound?.Invoke(this, itemFoundEventArgs);
                    if (itemFoundEventArgs.StopSearch)
                    {
                        yield break;
                    }
                    if (_matchPredicate(fileSystemInfo) && !itemFoundEventArgs.Exclude)
                    {
                        foreach (var nextFileSystemInfo in GetFileSystemInfo(fileSystemInfo.FullName))
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
