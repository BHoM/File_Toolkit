/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace BH.oM.Adapters.File
{
    [Description("A FileSystem-hosted File. It can include the content of the File.")]
    public class FSFile : BHoMObject, IFSContainer, IFile, ISizeableResource
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        [Description("Full path of parent Directory of the File. You can also specify a string path.")]
        public virtual FSDirectory ParentDirectory { get; set; }

        [Description("Name of the file, INCLUDING Extension.")]
        public override string Name { get; set; }

        [Description("Gets a value indicating whether a file exists.")]
        public virtual bool? Exists { get; set; } = null;

        [Description("Gets or sets a value that determines if the current file is read only.")]
        public virtual bool? IsReadOnly { get; set; } = null;

        [Description("The size of the File in bytes.")]
        public virtual int Size { get; set; } = 0;

        [Description("Attributes indicating if ReadOnly, Hidden, System File, etc.")]
        public virtual FileAttributes Attributes { get; set; }

        public virtual DateTime CreationTimeUtc { get; set; }
        public virtual DateTime ModifiedTimeUtc { get; set; }

        [Description("User owning the file, if any, or the user who created the File.")]
        public virtual string Owner { get; set; }

        [Description("The content of the file.")]
        public virtual List<object> Content { get; set; } = new List<object>();


        /***************************************************/
        /**** Implicit cast                             ****/
        /***************************************************/

        public static implicit operator FSFile(string fileFullPath)
        {
            if (String.IsNullOrWhiteSpace(fileFullPath))
                return null;

            FSFile bhomfile = new FSFile();
            FileInfo fi = new System.IO.FileInfo(fileFullPath);

            bhomfile.Name = fi.Name;
            bhomfile.ParentDirectory = fi.Directory.FullName;

            return bhomfile;
        }

        public static implicit operator FSFile(File file)
        {
            return (FSFile)Path.Combine(file.Location, file.Name);
        }

        /***************************************************/
        /**** ToString                                  ****/
        /***************************************************/

        public override string ToString()
        {
            return Path.Combine(this.ParentDirectory?.ToString() ?? "", this.Name ?? "");
        }
    }
}





