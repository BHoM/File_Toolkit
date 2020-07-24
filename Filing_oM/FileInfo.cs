using BH.oM.Base;
using BH.oM.Humans;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Filing
{
    [Description("Points to a File or Directory, but does not store the contents. Rehash of the .NET's class 'FileSystemInfo' in BHoM flavour.")]
    public class FileInfo : IFile
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        [Description("Full path of parent Directory.")]
        public virtual FileInfo ParentDirectory { get; set; }

        [Description("Name of the Directory or File.")]
        public virtual string Name { get; set; }

        [Description("Gets a value indicating whether the directory exists.")]
        public virtual bool Exists { get; set; } = false;

        [Description("Gets or sets a value that determines if the current file is read only.")]
        public virtual bool IsReadOnly { get; set; } = false;

        [Description("Gets the size, in bytes, of the current file.")]
        public virtual long Length { get; set; } = 0;


        [Description("Attributes indicating if ReadOnly, Hidden, System File, etc.")]
        public virtual FileAttributes Attributes { get; set; }

        public virtual DateTime CreationTime { get; set; }
        public virtual DateTime CreationTimeUtc { get; set; }
        public virtual DateTime LastAccessTime { get; set; }
        public virtual DateTime LastAccessTimeUtc { get; set; }
        public virtual DateTime LastWriteTime { get; set; }
        public virtual DateTime LastWriteTimeUtc { get; set; }

        [Description("User owning the file, if any, or the user who created the object File.")]
        public virtual Human Owner { get; set; }


        [Description(@"Root folder, such as '\', 'C:', or * '\\server\share'.")]
        public FileInfo Root { get; }


        /***************************************************/
        /**** Explicit cast                             ****/
        /***************************************************/

        public static explicit operator FileInfo(System.IO.DirectoryInfo directoryInfo)
        {
            return directoryInfo != null ? new FileInfo()
            {
                ParentDirectory = (FileInfo)directoryInfo.Parent,

                Name = directoryInfo.Name,

                Exists = directoryInfo.Exists,

                Attributes = directoryInfo.Attributes,
                CreationTime = directoryInfo.CreationTime,
                CreationTimeUtc = directoryInfo.CreationTimeUtc,
                LastAccessTime = directoryInfo.LastAccessTime,
                LastAccessTimeUtc = directoryInfo.LastAccessTimeUtc,
                LastWriteTime = directoryInfo.LastWriteTime,
                LastWriteTimeUtc = directoryInfo.LastWriteTimeUtc,
            } : null;
        }

        /***************************************************/
        /**** Implicit cast                             ****/
        /***************************************************/

        public static implicit operator FileInfo(string directoryFullPath)
        {
            if (!String.IsNullOrWhiteSpace(directoryFullPath))
                return (FileInfo)new System.IO.DirectoryInfo(directoryFullPath);
            else
                return null;
        }

    }
}
