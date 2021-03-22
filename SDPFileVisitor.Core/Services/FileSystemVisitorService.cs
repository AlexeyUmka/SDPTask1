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
        private readonly SystemFileInfoModel _fileInfoModel;

        public event FileSystemHandler<StartFinishEventArgs> SearchFinished;
        public event FileSystemHandler<StartFinishEventArgs> SearchStarted;

        public event FileSystemHandler<FileSystemInfoEventArgs> DirectoryFound;
        public event FileSystemHandler<FileSystemInfoEventArgs> FileFound;

        public event FileSystemHandler<FilteredFileSystemInfoEventArgs> DirectoryFiltered;
        public event FileSystemHandler<FilteredFileSystemInfoEventArgs> FileFiltered;

        public FileSystemVisitorService()
        {
            _startPath = "C:\\";
            _fileInfoModel = new SystemFileInfoModel(_startPath);
        }

        public FileSystemVisitorService(
            string startPath,
            Predicate<FileSystemInfo> matchPredicate)
        {
            _startPath = startPath;
            _matchPredicate = matchPredicate;
            _fileInfoModel = new SystemFileInfoModel(startPath);
        }

        public IEnumerable<FileSystemInfo> Search()
        {
            var startFinishEventArgs = new StartFinishEventArgs();
            var itemFoundEventArgs = new FileSystemInfoEventArgs();
            var itemFilteredEventArgs = new FilteredFileSystemInfoEventArgs();
            SearchStarted?.Invoke(this, startFinishEventArgs);
            foreach (var fileSystemInfo in _fileInfoModel)
            {
                if (startFinishEventArgs.StopSearch)
                {
                    SearchFinished?.Invoke(this, startFinishEventArgs);
                    yield break;
                }

                if (fileSystemInfo != null)
                {
                    itemFoundEventArgs.Name = fileSystemInfo.FullName;
                    itemFilteredEventArgs.Name = fileSystemInfo.FullName;
                    if (fileSystemInfo is FileInfo)
                    {
                        FileFound?.Invoke(this, itemFoundEventArgs);
                        if (_matchPredicate(fileSystemInfo) && !itemFilteredEventArgs.Exclude)
                        {
                            FileFiltered?.Invoke(this, itemFilteredEventArgs);
                            yield return fileSystemInfo;
                        }
                    }
                    else if (fileSystemInfo is DirectoryInfo)
                    {
                        DirectoryFound?.Invoke(this, itemFoundEventArgs);
                        if (_matchPredicate(fileSystemInfo) && !itemFilteredEventArgs.Exclude)
                        {
                            DirectoryFiltered?.Invoke(this, itemFilteredEventArgs);
                            yield return fileSystemInfo;
                        }
                    }
                }
            }

            SearchFinished?.Invoke(this, startFinishEventArgs);
        }
    }
}
