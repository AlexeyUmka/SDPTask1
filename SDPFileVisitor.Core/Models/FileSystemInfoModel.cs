using System.IO;

namespace SDPFileVisitor.Core.Models
{
    public class FileSystemInfoModel
    {
        public string Name { get; }
        public string FullName { get; }
        public string Extension { get; }
        public SystemItemType SystemItemType { get; }
        public FileSystemInfoModel(string name, string fullName, string extension, SystemItemType systemItemType)
        {
            Name = name;
            FullName = fullName;
            Extension = extension;
            SystemItemType = systemItemType;
        }
    }
}
