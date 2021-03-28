using System;
using System.Collections.Generic;
using SDPFileVisitor.Core.Models;

namespace SDPFileVisitor.Core.Comparers
{
    public class FileSystemInfoModelComparer : IEqualityComparer<FileSystemInfoModel>
    {
        public bool Equals(FileSystemInfoModel x, FileSystemInfoModel y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Name == y.Name && x.FullName == y.FullName && x.Extension == y.Extension && x.SystemItemType == y.SystemItemType;
        }

        public int GetHashCode(FileSystemInfoModel obj)
        {
            return HashCode.Combine(obj.Name, obj.FullName, obj.Extension, (int) obj.SystemItemType);
        }
    }
}