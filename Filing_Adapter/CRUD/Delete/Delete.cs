/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using BH.oM.Adapter;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Filing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BH.Engine.Filing;

namespace BH.Adapter.Filing
{
    public partial class FilingAdapter : BHoMAdapter
    {
        protected int Delete(FileDirRequest fdr, RemoveConfig removeConfig)
        {
            int deletedCount = 0;

            // Check if the request points to a single file.
            if (Query.IsExistingFile(fdr.ParentDirectory.IFullPath()))
            {
                // The FileDirRequest actually points to a single file.
                BH.oM.Filing.File file = (BH.oM.Filing.File)new FileInfo(fdr.ParentDirectory.IFullPath());

                // Delete the single file.
                if (Delete(file))
                    return 1;
            }

            List<IContent> queried = new List<IContent>();
            int retrievedFiles = 0, retrievedDirs = 0;
            WalkDirectories(queried, fdr, ref retrievedFiles, ref retrievedDirs, removeConfig.IncludeHiddenFiles, false);

            foreach (var item in queried)
            {
                if (Delete(item as dynamic, true))
                    deletedCount++;
            }

            return deletedCount;
        }

        protected IEnumerable<object> Delete(FileRequest fr)
        {
            return Read((FileDirRequest)fr);
        }

        protected IEnumerable<object> Delete(DirectoryRequest dr)
        {
            return Read((FileDirRequest)dr);
        }

        /***************************************************/

        private bool Delete(oM.Filing.File file, bool recordNote = false)
        {
            string fullPath = file.IFullPath();
            try
            {
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);

                    if (recordNote)
                        BH.Engine.Reflection.Compute.RecordNote($"File deleted: {fullPath}");

                    return true;
                }
                else
                {
                    BH.Engine.Reflection.Compute.RecordWarning($"File not found: {fullPath}");
                    return false;
                }

            }
            catch (IOException ioExp)
            {
                BH.Engine.Reflection.Compute.RecordError(ioExp.Message);
            }

            return false;
        }

        private bool Delete(oM.Filing.Directory directory, bool recordNote = false)
        {
            string fullPath = directory.IFullPath();
            try
            {
                if (System.IO.Directory.Exists(fullPath))
                {
                    System.IO.Directory.Delete(fullPath);

                    if (recordNote)
                        BH.Engine.Reflection.Compute.RecordNote($"Directory deleted: {fullPath}");

                    return true;
                }
                else
                {
                    BH.Engine.Reflection.Compute.RecordWarning($"Directory not found: {fullPath}");
                    return false;
                }
            }
            catch (IOException ioExp)
            {
                BH.Engine.Reflection.Compute.RecordError(ioExp.Message);
            }

            return false;
        }


    }
}

