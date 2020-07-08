﻿using BH.Engine.Filing;
using BH.Engine.Reflection;
using BH.oM.Adapter;
using BH.oM.Data.Requests;
using BH.oM.Filing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.Base;

namespace BH.Adapter.Filing
{
    public partial class FilingAdapter
    {
        /***************************************************/
        /**** Methods                                  *****/
        /***************************************************/

        public override IEnumerable<object> Pull(IRequest request, PullType pullType = PullType.AdapterDefault, ActionConfig actionConfig = null)
        {
            return Read(request as dynamic);
        }

        /***************************************************/

        private List<oM.Filing.IFileSystemInfo> Read(IFileDirRequest request)
        {
            // Convert to most generic type of FileInfo request.
            FileAndDirRequest fdr = null;
            if (request is FileAndDirRequest)
                fdr = (FileAndDirRequest)request;
            else
                fdr = BH.Engine.Filing.Create.FileDirRequest(request as dynamic);

            // Recursively walk the directories to retrieve File and Directory Info.
            List<oM.Filing.IFileSystemInfo> output = new List<IFileSystemInfo>();
            WalkDirectories(output, fdr);

            return output;
        }

        private void WalkDirectories(List<oM.Filing.IFileSystemInfo> output, FileAndDirRequest fdr, int retrievedFiles = 0, int retrievedDirs = 0)
        {
            // Recursion stop condition.
            if (fdr.MaxNesting == 0)
                return;

            // Look in directory and, if requested, recursively in subdirectories.
            System.IO.DirectoryInfo currentDir = new System.IO.DirectoryInfo(fdr.Directory.FullPath());
            System.IO.DirectoryInfo[] dirArray = currentDir.GetDirectories();
            foreach (System.IO.DirectoryInfo dir in dirArray)
            {
                oM.Filing.DirectoryInfo bhomDir = (oM.Filing.DirectoryInfo)dir;
                bhomDir.ParentDirectory = (oM.Filing.DirectoryInfo)dir.Parent;

                if (fdr.RetrieveDirectories)
                    if (!MaxItemsReached(fdr.MaxDirectories, retrievedDirs))
                    {
                        output.Add(bhomDir);
                        retrievedDirs += 1;
                    }

                if (fdr.RetrieveFiles)
                {
                    System.IO.FileInfo[] files;

                    try
                    {
                        files = dir.GetFiles("*.*");
                    }
                    // This is thrown if one of the files requires permissions greater than the application provides.
                    catch (UnauthorizedAccessException e)
                    {
                        // Write out the message and continue.
                        BH.Engine.Reflection.Compute.RecordNote(e.Message);
                    }
                }
                foreach (var f in dir.GetFiles())
                {
                    if (!MaxItemsReached(fdr.MaxFiles, retrievedFiles))
                    {
                        output.Add((oM.Filing.FileInfo)f);
                        retrievedFiles += 1;
                    }
                    else
                        break;
                }

                // Recurse if requested, and if the limits are not exceeded.
                if (fdr.IncludeSubdirectories == true && MaxItemsReached(fdr.MaxFiles, retrievedFiles, fdr.MaxDirectories, retrievedDirs))
                {
                    FileAndDirRequest fdrRecurse = BH.Engine.Base.Query.ShallowClone(fdr);
                    fdrRecurse.Directory = bhomDir;
                    fdrRecurse.MaxNesting -= 1;

                    Read(fdrRecurse);
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
            return !MaxItemsReached(maxFiles, retrievedFilesCount) && !MaxItemsReached(maxDirs, retrivedDirsCount);
        }




    }
}
