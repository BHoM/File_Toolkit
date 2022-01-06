/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BH.Engine.Adapters.File;

namespace BH.Adapter.File
{
    public partial class FileAdapter : BHoMAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static int Delete(RemoveRequest rr, RemoveConfig removeConfig)
        {
            int deletedCount = 0;

            if (!rr.ToRemove.Any())
                return deletedCount;

            foreach (string fullPath in rr.ToRemove)
            {
                BH.oM.Adapters.File.FSDirectory dir = null;
                if (fullPath.IsExistingDir())
                    dir = ReadDirectory(fullPath);

                BH.oM.Adapters.File.FSFile file = null;
                if (fullPath.IsExistingFile())
                    file = ReadFile(fullPath);

                bool success = false;

                if (dir != null) 
                    success |= DeleteDirectory(fullPath);


                if (file != null)
                    success = DeleteFile(fullPath);

                if (success)
                    deletedCount++;
            }

            return deletedCount;
        }


        /***************************************************/

        public static bool DeleteFile(string filePath, bool recordNote = false)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);

                    if (recordNote)
                        BH.Engine.Reflection.Compute.RecordNote($"File deleted: {filePath}");

                    return true;
                }
                else
                {
                    BH.Engine.Reflection.Compute.RecordWarning($"File not found: {filePath}");
                    return false;
                }

            }
            catch (IOException ioExp)
            {
                BH.Engine.Reflection.Compute.RecordError(ioExp.Message);
            }

            return false;
        }

        /***************************************************/

        public static bool DeleteDirectory(string directoryPath, bool recordNote = false)
        {
            try
            {
                if (System.IO.Directory.Exists(directoryPath))
                {
                    System.IO.Directory.Delete(directoryPath);

                    if (recordNote)
                        BH.Engine.Reflection.Compute.RecordNote($"Directory deleted: {directoryPath}");

                    return true;
                }
                else
                {
                    BH.Engine.Reflection.Compute.RecordWarning($"Directory not found: {directoryPath}");
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



