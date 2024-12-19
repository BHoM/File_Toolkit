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

using BH.Engine.Adapters.File;
using BH.oM.Adapter;
using BH.oM.Base;
using BH.oM.Adapters.File;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace BH.Adapter.File
{
    public partial class FileAdapter : BHoMAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public IEnumerable<object> Read(FileDirRequest fdr, PullConfig pullConfig)
        {
            // Copy for immutability in UI
            FileDirRequest fdrCopy = BH.Engine.Base.Query.ShallowClone(fdr);

            // Recursively walk the directories to retrieve File and Directory Info.
            List<IFSInfo> output = new List<IFSInfo>();

            List<FSFile> files = new List<FSFile>();
            List<FSDirectory> dirs = new List<FSDirectory>();

            WildcardPattern wildcardPattern = null;
            if (!Modify.ProcessFileDirRequest(fdrCopy, out wildcardPattern))
                return null;

            int retrievedFiles = 0, retrievedDirs = 0;
            WalkDirectories(files, dirs, fdrCopy, ref retrievedFiles, ref retrievedDirs, pullConfig.IncludeHiddenFiles, pullConfig.IncludeSystemFiles, wildcardPattern);

            output.AddRange(dirs);
            output.AddRange(files);

            // If a sort order is applied, sort separately files and dirs,
            // then return the maxItems of each of those.
            if (fdrCopy.SortOrder != SortOrder.Default)
            {
                output = Query.SortOrder(output, fdrCopy.SortOrder);

                files = output.OfType<FSFile>().Take(fdrCopy.MaxFiles == -1 ? output.Count : fdrCopy.MaxFiles).ToList();
                dirs = output.OfType<FSDirectory>().Take(fdrCopy.MaxDirectories == -1 ? output.Count : fdrCopy.MaxDirectories).ToList();
            }

            if (fdrCopy.IncludeFileContents)
                files.ForEach(f => ReadAndAddContent(f));

            output = new List<IFSInfo>();

            if (fdrCopy.SortOrder != SortOrder.Default && fdrCopy.SortOrder != SortOrder.ByName)
            {
                output.AddRange(files);
                output.AddRange(dirs);
            }
            else
            {
                output.AddRange(dirs);
                output.AddRange(files);
            }

            return output;
        }

        /***************************************************/

        public IEnumerable<object> Read(FileRequest fr, PullConfig pullConfig)
        {
            // Convert to the most generic type of Request.
            return Read((FileDirRequest)fr, pullConfig);
        }

        /***************************************************/

        public IEnumerable<object> Read(DirectoryRequest dr, PullConfig pullConfig)
        {
            // Convert to the most generic type of Request.
            return Read((FileDirRequest)dr, pullConfig);
        }
    }
}






