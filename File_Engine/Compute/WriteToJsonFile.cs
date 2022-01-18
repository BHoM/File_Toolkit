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

using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BH.Engine.Serialiser;
using BH.Engine.Adapters.File;
using BH.oM.Adapters.File;

namespace BH.Engine.Adapters.File
{
    public partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static bool WriteToJsonFile(List<object> objects, string filePath, bool replace = false, bool active = false)
        {
            if (!active)
                return false;

            if (objects == null)
                return false;

            // Make sure no invalid chars are present.
            filePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));

            // Make sure the .json extension is present.
            if (!filePath.ToLower().EndsWith(".json") || string.IsNullOrWhiteSpace(filePath))
            {
                BH.Engine.Base.Compute.RecordError($"The filePath `{filePath}` must point to a JSON file. Make sure the file extension `.json` is present.");
                return false;
            }

            // If the file exists already, stop execution if `replace` is not true.
            bool fileExisted = System.IO.File.Exists(filePath);
            if (replace == false && fileExisted)
            {
                BH.Engine.Base.Compute.RecordWarning($"The file `{filePath}` exists already. To replace its content, set `{nameof(replace)}` to true.");
                return false;
            }

            // Serialize to json and create the file and directory.
            string json = Engine.Adapters.File.Convert.ToJsonArray(objects);
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
            fileInfo.Directory.Create(); // If the directory already exists, this method does nothing.

            try
            {
                System.IO.File.WriteAllText(filePath, json);
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError($"Error writing to file:\n\t{e.ToString()}");

                return false;
            }

            return true;
        }
    }
}



