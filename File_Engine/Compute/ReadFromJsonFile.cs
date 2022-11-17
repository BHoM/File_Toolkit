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
using System.Collections;
using BH.oM.Adapter;
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Engine.Adapters.File
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Read a JSON-serialised file and output any data or object included in it.")]
        [Input("filePath", "Path to the file.")]
        [Input("active", "Boolean used to trigger the function.")]
        public static List<object> ReadFromJsonFile(string filePath, bool active = false)
        {
            if (!active)
                return new List<object>();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                BH.Engine.Base.Compute.RecordError($"The filePath `{filePath}` must not be empty.");
                return new List<object>();
            }

            // Make sure no invalid chars are present.
            string fullPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
            
            bool fileExisted = System.IO.File.Exists(fullPath);
            if (!fileExisted)
            {
                BH.Engine.Base.Compute.RecordError($"The file `{fullPath}` does not exist.");
                return new List<object>();
            }

            string jsonText = System.IO.File.ReadAllText(fullPath);
            object converted = BH.Engine.Serialiser.Convert.FromJson(jsonText);

            // Check if there is any ObjectWrapper that was used to allow writing of non-IObjects, like primitive types (numbers/strings).
            List<object> convertedList = new List<object>();
            IEnumerable ienum = converted as IEnumerable;
            if (ienum != null)
            {
                foreach (var obj in ienum)
                {
                    ObjectWrapper objectWrapper = obj as ObjectWrapper;
                    if (objectWrapper != null)
                        convertedList.Add(objectWrapper.WrappedObject);
                    else
                        convertedList.Add(obj);
                }

                return convertedList;
            }

            return new List<object>() { converted };
        }
    }
}