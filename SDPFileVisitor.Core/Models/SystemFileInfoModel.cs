using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SDPFileVisitor.Core.Models
{
    public class SystemFileInfoModel : IEnumerable<FileSystemInfo>
    {
        private readonly string _rootDirectoryPath;

        public SystemFileInfoModel(string rootDirectoryPath)
        {
            _rootDirectoryPath = rootDirectoryPath;
        }

        public IEnumerator<FileSystemInfo> GetEnumerator()
        {
            var directory = new DirectoryInfo(_rootDirectoryPath);
            return GetFileSystemInfo(directory).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerable<FileSystemInfo> GetFileSystemInfo(DirectoryInfo directoryInfo)
        {
            var allElements = directoryInfo.GetFileSystemInfos();
            foreach (var fileSystemInfo in allElements)
            {
                if (fileSystemInfo is FileInfo)
                {
                    yield return fileSystemInfo;
                }
                else if (fileSystemInfo is DirectoryInfo nextDirectory)
                {
                    foreach (var nextFileSystemInfo in GetFileSystemInfo(nextDirectory))
                    {
                        yield return nextFileSystemInfo;
                    }

                    yield return fileSystemInfo;
                }
            }
        }
    }
}
