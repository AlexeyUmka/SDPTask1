using System.Collections.Generic;
using System.IO;
using SDPFileVisitor.Core.Models;

namespace SDPFileVisitor.Core.Interfaces
{
    public interface IDirectoryInfoService
    {
        public IEnumerable<FileSystemInfoModel> GetFileSystemInfos(string directoryPath);
    }
}
