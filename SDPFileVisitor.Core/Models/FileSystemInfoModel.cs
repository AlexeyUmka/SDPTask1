using System.IO;
using SDPFileVisitor.Core.Comparers;

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

        public override bool Equals(object? obj)
        {
            var comparer = new FileSystemInfoModelComparer();
            return comparer.Equals(this, obj as FileSystemInfoModel);
        }

        public override int GetHashCode()
        {
            var comparer = new FileSystemInfoModelComparer();
            return comparer.GetHashCode(this);
        }
    }
}
