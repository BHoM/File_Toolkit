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
using System.IO;
using System.Linq;
using BH.oM.Adapter;
using BH.Engine.Adapters.File;
using BH.oM.Adapters.File;
using System.Text.RegularExpressions;
using System.Management.Automation;

namespace BH.Adapter.File
{
    public partial class FileAdapter
    {
        private void WalkDirectories(List<FSFile> files, List<FSDirectory> dirs, FileDirRequest fdr,
            ref int filesCount, ref int dirsCount,
            bool inclHidFiles = false, bool inclSysFiles = false, WildcardPattern wildcardPattern = null)
        {
            // Recursion stop condition.
            if (fdr.MaxNesting == 0)
                return;

            // Look in directory and, if requested, recursively in subdirectories.
            if (string.IsNullOrWhiteSpace(fdr.Location))
            {
                BH.Engine.Base.Compute.RecordError($"Missing parameter {nameof(fdr.Location)} from the request.");
                return;
            }

            System.IO.DirectoryInfo selectedDir = new System.IO.DirectoryInfo(fdr.Location.IFullPath());
            System.IO.DirectoryInfo[] dirArray = new System.IO.DirectoryInfo[] { };

            // Check if the location points to a single file.
            // To point to a single file, the location must not be a wildcard (therefore wildcardPattern must be null)
            // and it must have an extension.
            bool isSingleFile = Path.HasExtension(selectedDir.FullName) && wildcardPattern == null;

            if (!isSingleFile) // If the location points to a directory, populate the list of folders there.
                dirArray = selectedDir.GetDirectories();
            else // if the location points to a single file, the selected directory is its parent.
                selectedDir = new System.IO.DirectoryInfo(Path.GetDirectoryName(selectedDir.FullName));

            foreach (System.IO.DirectoryInfo di in dirArray)
            {
                oM.Adapters.File.FSDirectory bhomDir = ReadDirectory(di.FullName, inclHidFiles, inclSysFiles);
                if (bhomDir == null)
                    continue;

                bhomDir.ParentDirectory = di.Parent.ToFiling();

                if (fdr.Exclusions != null && fdr.Exclusions.Contains(bhomDir))
                    continue;

                if (fdr.IncludeDirectories && wildcardPattern == null)
                {
                    if (fdr.SortOrder != SortOrder.Default || !MaxItemsReached(fdr.MaxDirectories, dirsCount))
                    {
                        // The limit in number of item retrieved in WalkDirectories applies only if there is no sortOrder applied.
                        // If a sortOrder is applied, the maxItems must be applied after the sorting is done (outside of WalkDirectories)

                        // Check exclusions
                        if (fdr.Exclusions != null && fdr.Exclusions.Contains(bhomDir))
                            continue;

                        // Check Wildcard matches - DISABLED: only allow wildcards in filename. Too complicated otherwise.
                        //if (!wildcardPattern?.IsMatch(bhomDir.Name) ?? false) 
                        //    continue;

                        dirs.Add(bhomDir);
                        dirsCount += 1;
                    }
                }


                // Recurse if requested, and if the limits are not exceeded.
                if (fdr.SearchSubdirectories == true && !MaxItemsReached(fdr.MaxFiles, filesCount, fdr.MaxDirectories, dirsCount))
                {
                    FileDirRequest fdrRecurse = BH.Engine.Base.Query.ShallowClone(fdr);
                    fdrRecurse.Location = bhomDir.IFullPath();
                    fdrRecurse.MaxNesting -= 1;

                    WalkDirectories(files, dirs, fdrRecurse, ref filesCount, ref dirsCount, inclHidFiles, inclSysFiles, wildcardPattern);
                }
            }

            if (fdr.IncludeFiles)
            {
                System.IO.FileInfo[] fileInfos = new System.IO.FileInfo[1];

                try
                {
                    if (isSingleFile)
                        fileInfos[0] = new FileInfo(fdr.Location.IFullPath());
                    else
                        fileInfos = selectedDir.GetFiles("*.*");
                }
                // This is thrown if one of the files requires permissions greater than the application provides.
                catch (UnauthorizedAccessException e)
                {
                    // Write out the message and continue.
                    BH.Engine.Base.Compute.RecordNote(e.Message);
                }

                foreach (var fi in fileInfos)
                {
                    if (fdr.SortOrder != SortOrder.Default || !MaxItemsReached(fdr.MaxFiles, filesCount))
                    {
                        // The limit in number of item retrieved in WalkDirectories applies only if there is no sortOrder applied.
                        // If a sortOrder is applied, the maxItems must be applied after the sorting is done (outside of WalkDirectories)

                        // Check exclusions
                        if (fdr.Exclusions != null && fdr.Exclusions.Contains(fi.ToFiling()))
                            continue;

                        // Check Wildcard matches
                        if (!wildcardPattern?.IsMatch(fi.Name) ?? false)
                            continue;

                        // When reading the file, do not retrieve content.
                        // Content must be retrieved after WalkDirectories has run.
                        // This is because additional filtering might be done later.
                        oM.Adapters.File.FSFile omFile = ReadFile(fi.FullName, false, inclHidFiles, inclSysFiles);

                        if (omFile != null)
                        {
                            files.Add(omFile);
                            filesCount += 1;
                        }
                    }
                    else
                        break;
                }
            }
        }

        /***************************************************/

        private bool MaxItemsReached(int maxItems, int retrievedItemsCount)
        {
            return maxItems != -1 && retrievedItemsCount >= maxItems;
        }

        private bool MaxItemsReached(int maxFiles, int retrievedFilesCount, int maxDirs, int retrivedDirsCount)
        {
            return MaxItemsReached(maxFiles, retrievedFilesCount) || MaxItemsReached(maxDirs, retrivedDirsCount);
        }
    }
}





