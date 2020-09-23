using BH.oM.Base;
using BH.oM.Humans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using BH.oM.Data.Requests;

namespace BH.oM.Adapters.Filing
{
    [Description("Used to query Directories or Files.")]
    public class FileDirRequest : IFileRequest, IDirectoryRequest
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        [Description("Directory and/or Files from this location will be queried.")]
        public virtual string Location { get; set; } = "";

        [Description("Whether to include Files.")]
        public virtual bool IncludeFiles { get; set; } = true;

        [Description("Whether to include Directories.")]
        public virtual bool IncludeDirectories { get; set; } = true;

        [Description("If enabled, look also in subdirectories.")]
        public virtual bool SearchSubdirectories { get; set; } = false;

        [Description("Whether to include the contents of the Files.")]
        public virtual bool IncludeFileContents { get; set; } = false;

        [Description("If IncludeSubdirectories is true, this sets the maximum subdirectiory nesting level to look in." +
            "\nDefaults to -1 which corresponds to no limit.")]
        public virtual int MaxNesting { get; set; } = -1;

        [Description("Sets the maximum number of Files to retrieve." +
            "\nDefaults to -1 which corresponds to no limit.")]
        public virtual int MaxFiles { get; set; } = -1;

        [Description("Sets the maximum number of Directories to retrieve." +
            "\nDefaults to -1 which corresponds to no limit.")]
        public virtual int MaxDirectories { get; set; } = -1;

        [Description("These files or directories will be excluded from the results. You can also specify string Full Paths.")]
        public virtual List<IFSInfo> Exclusions { get; set; } = new List<IFSInfo>();

        /***************************************************/
        /**** Implicit cast                             ****/
        /***************************************************/

        public static implicit operator FileDirRequest(string fullPath)
        {
            if (!String.IsNullOrWhiteSpace(fullPath))
                return new FileDirRequest() { Location = fullPath };
            else
                return null;
        }

        public static implicit operator FileDirRequest(FileRequest fr)
        {
            return new FileDirRequest() {
                Location = fr.Location,
                IncludeDirectories = false,
                IncludeFiles = true,
                SearchSubdirectories = fr.SearchSubdirectories,
                MaxFiles = fr.MaxFiles,
                MaxNesting = fr.MaxNesting,
                IncludeFileContents = fr.IncludeFileContents
            };
        }

        public static implicit operator FileDirRequest(DirectoryRequest dr)
        {
            return new FileDirRequest()
            {
                Location = dr.Location,
                IncludeDirectories = true,
                IncludeFiles = false,
                SearchSubdirectories = dr.SearchSubdirectories,
                MaxDirectories = dr.MaxDirectories,
                MaxNesting = dr.MaxNesting,
                Exclusions = dr.Exclusions
            };
        }
    }
}
