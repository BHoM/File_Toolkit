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
using MongoDB.Bson;
using System.Collections;
using BH.oM.Adapter;

namespace BH.Engine.Adapters.File
{
    public partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<object> ReadFromJsonFile(string filePath, bool active = false)
        {
            if (!active)
                return new List<object>();

            if (!filePath.EndsWith(".json") || string.IsNullOrWhiteSpace(filePath))
            {
                BH.Engine.Base.Compute.RecordError($"The filePath `{filePath}` must point to a JSON file. Make sure the file extension `.json` is present.");
                return new List<object>();
            }

            string fullPath = filePath.IFullPath();
            bool fileExisted = System.IO.File.Exists(fullPath);

            if (!fileExisted)
            {
                BH.Engine.Base.Compute.RecordError($"The file `{fullPath}` does not exist.");
                return new List<object>();
            }

            string jsonText = System.IO.File.ReadAllText(fullPath);
            object converted = null;

            if (!FromJson(jsonText, out converted))
            {
                Engine.Base.Compute.RecordWarning($"The content of file `{fullPath}` is not a supported json format.");
                return new List<object>();
            }

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

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static bool FromJson(string json, out object result)
        {
            result = null;

            if (json == "")
                return false;

            if (json.StartsWith("{"))
            {
                BsonDocument document;
                if (BsonDocument.TryParse(json, out document))
                {
                    result = BH.Engine.Serialiser.Convert.FromBson(document);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (json.StartsWith("["))
            {
                BsonArray array = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonArray>(json);

                try
                {
                    result = array.Select(b => b.IsBsonDocument ? BH.Engine.Serialiser.Convert.FromBson(b.AsBsonDocument) : BH.Engine.Serialiser.Convert.FromJson(b.ToString())).ToList();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
    }
}