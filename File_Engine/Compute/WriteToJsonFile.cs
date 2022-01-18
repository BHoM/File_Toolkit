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
using System.ComponentModel;
using BH.oM.Base.Attributes;
using BH.oM.Adapter;

namespace BH.Engine.Adapters.File
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Write a JSON-serialised file with the input data or objects.")]
        [Input("filePath", "Path to the file.")]
        [Input("replace", "If the file exists, you need to set this to true in order to allow overwriting it.")]
        [Input("active", "Boolean used to trigger the function.")]
        public static bool WriteToJsonFile(List<object> objects, string filePath, bool replace = false, bool active = false)
        {
            if (!active || objects == null)
                return false;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                BH.Engine.Base.Compute.RecordError($"The filePath `{filePath}` must not be empty.");
                return false;
            }

            // Make sure no invalid chars are present.
            filePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));

            // If the file exists already, stop execution if `replace` is not true.
            bool fileExisted = System.IO.File.Exists(filePath);
            if (!replace && fileExisted)
            {
                BH.Engine.Base.Compute.RecordWarning($"The file `{filePath}` exists already. To replace its content, set `{nameof(replace)}` to true.");
                return false;
            }

            // Serialise to json and create the file and directory.
            string json = ToJsonArrayWrappingNonObjects(objects);
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

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        // Writes to a JSON Array and wraps non-objects (primitive types) to allow to serialise them.
        private static string ToJsonArrayWrappingNonObjects(this List<object> objects)
        {
            string json = "";
            List<string> allLines = new List<string>();

            // Parse all objects.
            foreach (object obj in objects)
            {
                // Skip nulls.
                if (obj == null)
                    continue;

                // Wrap non-IObjects.
                IObject toWrite = obj as IObject;
                if (toWrite == null)
                    toWrite = WrapInBHoMObject(obj);

                // Add to serialized list.
                allLines.Add(toWrite.ToJson() + ",");
            }

            // Remove the trailing comma if there is only one element.
            allLines[allLines.Count - 1] = allLines[allLines.Count - 1].Remove(allLines[allLines.Count - 1].Length - 1);

            // Join all between square brackets to make a valid JSON array.
            json = String.Join(Environment.NewLine, allLines);
            json = "[" + json + "]";

            return json;
        }

        /***************************************************/

        private static ObjectWrapper WrapInBHoMObject(object obj)
        {
            return new ObjectWrapper() { WrappedObject = obj, Name = obj.GetType().FullName };
        }
    }
}



