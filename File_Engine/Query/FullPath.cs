/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.File;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Adapters.File
{
    public static partial class Query
    {
        /***************************************************/
        /*** Methods                                     ***/
        /***************************************************/

        [Description("Get the full path. " +
            "If the input is a string path containing wildcard symbols, return only the upper portion of the path that doesn't have any.")]
        public static string IFullPath(this object obj)
        {
            return obj != null ? FullPath(obj as dynamic) ?? "" : "";
        }

        private static string FullPath(this IFSInfo baseInfo)
        {
            if (baseInfo.ParentDirectory == null)
                return baseInfo.Name;

            if (!IsAcyclic(baseInfo))
                BH.Engine.Base.Compute.RecordError("Circular directory hierarchy found.");

            return baseInfo.ToString();
        }

        private static string FullPath(this IResourceRequest fdr)
        {
            return FullPath(fdr.Location);
        }

        private static string FullPath(this ILocatableResource fdr)
        {
            if (fdr?.Location == null)
                return null;

            string location = Path.Combine(fdr.Location, string.IsNullOrWhiteSpace(fdr.Name) ? "" : fdr.Name);

            if (location.Contains("/") && location.Contains("\\"))
            {
                // If the location has inconsistent separators, assume the format of local windows FileSystem.
                location = location.Replace("/", "\\");
            }

            return location;
        }

        private static string FullPath(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            path = path.Replace("/", "\\");

            FileInfo fi = null;
            DirectoryInfo di = null;

            if (System.Management.Automation.WildcardPattern.ContainsWildcardCharacters(path))
            {
                // If path contains wildcard chars, return only up to the parent folder that doesn't have any.
                string allButLastSegment = path.Remove(path.Count() - Path.GetFileName(path).Count());
                return FullPath(allButLastSegment);
            }

            di = new DirectoryInfo(path);

            if (di?.Exists ?? false)
                return di.FullName;

            if (di != null && !di.Exists && !string.IsNullOrWhiteSpace(Path.GetFileName(path)))
            {
                // If the full path does not exist but there is a FileName, we might be using a Wildcard in the FileName.
                // Extract only the directory part.
                di = new DirectoryInfo(Path.GetDirectoryName(path));
            }

            try
            {
                fi = new FileInfo(path);
            }
            catch
            {
                if (di == null || !di.Exists)
                    BH.Engine.Base.Compute.RecordError($"Invalid path provided:\n`{path}`");
            }


            if (fi?.Exists ?? false)
                return fi.FullName;

            // If the following is true, we assume we're using a Wildcard.
            if (!fi?.Exists ?? false && di != null && di.Exists)
                return di.FullName;

            return null;
        }

        //Fallback
        private static string FullPath(object fileOrDir)
        {
            BH.Engine.Base.Compute.RecordError($"Can not compute the FullPath for an object of type `{fileOrDir.GetType().Name}`.");
            return null;
        }

        /***************************************************/
    }
}



