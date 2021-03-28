using System.Collections.Generic;
using System.IO;
using System.Linq;
using SDPFileVisitor.Core.Interfaces;
using SDPFileVisitor.Core.Models;

namespace SDPFileVisitor.Core.Services
{
    public class DirectoryInfoService : IDirectoryInfoService
    {
        public IEnumerable<FileSystemInfoModel> GetFileSystemInfos(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            return directoryInfo.GetFileSystemInfos().Select(x =>
            {
                var systemItemType = x switch
                {
                    FileInfo file => SystemItemType.File,
                    DirectoryInfo directory => SystemItemType.Directory,
                    _ => SystemItemType.Undefined
                };
                return new FileSystemInfoModel(x.Name, x.FullName, x.Extension, systemItemType);
            });
        }
    }
}
