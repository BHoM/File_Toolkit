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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BH.Engine.Serialiser;
using BH.oM.Adapter;
using BH.Engine.Adapters.File;
using BH.oM.Adapters.File;

namespace BH.Adapter.File
{
    public partial class FileAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static oM.Adapters.File.FSFile ReadFile(FileRequest fr, PullConfig pc)
        {
            string fullPath = fr.Location.IFullPath();

            return ReadFile(fullPath, fr.IncludeFileContents, pc.IncludeHiddenFiles, pc.IncludeSystemFiles);
        }

        /***************************************************/

        public static oM.Adapters.File.FSFile ReadFile(string fullPath, bool inclFileContent = false, bool inclHidFiles = false, bool inclSysFiles = false)
        {
            // Perform the "Read" = get the System.FileInfo, which will be the basis for our oM.Adapters.Filing.File
            FileInfo fi = new FileInfo(fullPath);

            // Check if we have attributes
            if ((int)fi.Attributes != -1)
            {
                // Checks on config
                if (!inclHidFiles && (fi.Attributes & FileAttributes.Hidden) > 0)
                    return null;

                if (!inclSysFiles && (fi.Attributes & FileAttributes.System) > 0)
                    return null;

                // Checks on FileInfo
                if ((fi.Attributes & FileAttributes.Directory) <= 0 && !fi.Exists)
                    return null;
            }

            // Convert the FileInfo to our oM.Adapters.Filing.File
            oM.Adapters.File.FSFile file = fi.ToFiling();

            // Add author data if possible
            AddAuthor(file);

            // Add content data if requested and possible
            if (inclFileContent)
                ReadAndAddContent(file);

            return file;
        }

        /***************************************************/

        public static oM.Adapters.File.FSDirectory ReadDirectory(string fullPath, bool inclHidDirs = false, bool inclSysDirs = false, bool includeFolderContent = false)
        {
            // Perform the "Read" = get the System.DirectoryInfo, which will be the basis for our oM.Adapters.Filing.Directory
            DirectoryInfo di = new DirectoryInfo(fullPath);

            // Checks on config
            if (!inclHidDirs && (di.Attributes & FileAttributes.Hidden) > 0)
                return null;

            if (!inclSysDirs && (di.Attributes & FileAttributes.System) > 0)
                return null;

            // Checks on FileInfo
            if ((di.Attributes & FileAttributes.Directory) <= 0 && !di.Exists)
                return null;

            // Convert the FileInfo to our oM.Adapters.Filing.File
            oM.Adapters.File.FSDirectory dir = di.ToFiling();

            // Add author data if possible
            AddAuthor(dir);

            if (includeFolderContent)
                AddContent(dir);

            return dir;
        }
    }
}



