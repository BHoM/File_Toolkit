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

using BH.Engine.Serialiser;
using BH.oM.Adapter;
using BH.oM.Adapters.File;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Adapters.File
{
    public static partial class Convert
    {
        /***************************************************/
        /*** Methods                                     ***/
        /***************************************************/

        [Description("Serializes the objects and returns a json array.")]
        [Input("objects", "Objects you want to serialize and concatenate into a single JSON array.")]
        [Output("jsonArray", "A JSON array with the serialized objects.")]
        public static string ToJsonArray(this List<object> objects)
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

        private static ObjectWrapper WrapInBHoMObject(object obj)
        {
            return new ObjectWrapper() { WrappedObject = obj, Name = obj.GetType().FullName };
        }
    }
}


