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
        /**** Private Methods                           ****/
        /***************************************************/

        private static void AddAuthor(oM.Adapters.File.IFSContainer retrievedFile)
        {
            string fullPath = retrievedFile.IFullPath();

            // Retrieve additional data: author/owner
            if ((retrievedFile.Attributes & FileAttributes.System) > 0)
                retrievedFile.Owner = "System";
            else
            {
                try
                {
                    retrievedFile.Owner = System.IO.File.GetAccessControl(fullPath)
                                            .GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();
                }
                catch
                {
                    BH.Engine.Base.Compute.RecordNote($"Cannot retrieve Owner of {retrievedFile.GetType().Name} `{fullPath}`");
                }
            }
        }

        /***************************************************/

        private static void ReadAndAddContent(oM.Adapters.File.FSFile retrievedFile)
        {
            string fullPath = retrievedFile.IFullPath();

            var content = ReadContent(fullPath);
            retrievedFile.Content.AddRange(content);
        }

        /***************************************************/

        private static void AddContent(oM.Adapters.File.FSDirectory retrievedDir)
        {
            string fullPath = retrievedDir.IFullPath();

            var content = new DirectoryInfo(fullPath).GetFiles("*.*");

            retrievedDir.Content.AddRange(content.Cast<IFSInfo>());
        }
    }
}



