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

using BH.oM.Base;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BH.Engine.Serialiser;
using BH.oM.Adapter;
using BH.Engine.Adapters.File;
using BH.oM.Adapters.File;
using System.Text.Json;
using BH.Engine.Diffing;
using BH.oM.Diffing;

namespace BH.Adapter.File
{
    public partial class FileAdapter : BHoMAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public FSFile CreateJson(FSFile file, PushType pushType, PushConfig pushConfig)
        {
            string fullPath = file.IFullPath();
            bool fileExisted = System.IO.File.Exists(fullPath);

            // Put together all of the file content.
            List<string> allLines = new List<string>();
            string json = "";

            if (file.Content != null)
            {
                if (!pushConfig.UseDatasetSerialization)
                {
                    allLines.AddRange(file.Content.Where(c => c != null).Select(obj => obj.ToJson() + ","));

                    // Remove the trailing comma if there is only one element.
                    if (string.IsNullOrWhiteSpace(allLines.ElementAtOrDefault(1)) && !string.IsNullOrWhiteSpace(allLines.FirstOrDefault()))
                        allLines[0] = allLines[0].Remove(allLines.FirstOrDefault().Length - 1);

                    // Join all between square brackets to make a valid JSON array.
                    json = String.Join(Environment.NewLine, allLines);
                    json = "[" + json + "]";
                }
                else
                {
                    // Use the non-JSON compliant "Dataset" serialization style.
                    // This is a newline-separated concatenation of individual JSON-serialized objects.
                    allLines.AddRange(file.Content.Where(c => c != null).Select(obj => obj.ToJson()));
                    json = String.Join(Environment.NewLine, allLines);
                }
            }

            if (pushConfig.BeautifyJson)
                json = BeautifyJson(json);

            bool filecreated = true;
            try
            {
                // Clarify if we are considering the Push in terms of content or of Files.
                if (string.IsNullOrWhiteSpace(m_defaultFilePath)) // We are talking about Files/Directories.
                {
                    if (pushType == PushType.DeleteThenCreate)
                    {
                        if (fileExisted)
                            System.IO.File.Delete(fullPath);

                        WriteJsonFile(fullPath, json, true);
                    }
                    else if (pushType == PushType.UpdateOnly)
                    {
                        // Overwrite existing file.
                        if (fileExisted)
                            WriteJsonFile(fullPath, json, true);
                        else
                            BH.Engine.Reflection.Compute.RecordNote($"File {fullPath} was not updated as no file existed at that location.");
                    }
                    else if (pushType == PushType.UpdateOrCreateOnly)
                    {
                        // Overwrite existing file. If it doesn't exist, create it.
                        WriteJsonFile(fullPath, json, true);
                    }
                    else if (pushType == PushType.CreateOnly || pushType == PushType.CreateNonExisting)
                    {
                        // Create only if file didn't exist. Do not touch existing ones.
                        if (!fileExisted)
                            WriteJsonFile(fullPath, json, true); 
                        else
                            BH.Engine.Reflection.Compute.RecordNote($"File {fullPath} was not created as it existed already (Pushtype {pushType.ToString()} was specified).");
                    }
                    else
                    {
                        BH.Engine.Reflection.Compute.RecordWarning($"The specified Pushtype of {pushType.ToString()} is not supported for .json files.");
                        filecreated = false;
                    }
                }
                else // We are talking about File content.
                {
                    if (pushType == PushType.DeleteThenCreate)
                    {
                        BH.Engine.Reflection.Compute.RecordNote($"Replacing entire content of file `{fullPath}`.");

                        // Replace all content.
                        WriteJsonFile(fullPath, json, true);
                    }
                    else if (pushType == PushType.CreateOnly || pushType == PushType.CreateNonExisting || pushType == PushType.UpdateOnly || pushType == PushType.UpdateOrCreateOnly)
                    {
                        // Should be refactored to cover distinct use cases for CreateNonExisting, UpdateOnly, UpdateOrCreateOnly
                        if (!fileExisted)
                            WriteJsonFile(fullPath, json, true);
                        else
                        {
                            BH.Engine.Reflection.Compute.RecordNote($"Appending content to file `{fullPath}`.");
                            WriteJsonFile(fullPath, json, false);
                        }
                    }
                    else if (pushType == PushType.CreateNonExisting) 
                    {
                        // Currently captured by CreateOnly.

                        // The following ideally should be the default behaviour of the IDiffing method.

                        //IEnumerable<object> allReadContent = ReadContent(fullPath);
                        //IEnumerable<IBHoMObject> bHoMObjects_read = allReadContent.OfType<IBHoMObject>();
                        //IEnumerable<object> genericObjs_read = allReadContent.Except(bHoMObjects_read);

                        //IEnumerable<IBHoMObject> readBhomObjects_hashAssigned = BH.Engine.Diffing.Modify.SetHashFragment(bHoMObjects_read);

                        //IEnumerable<IBHoMObject> bHoMObjects_create = file.Content.OfType<IBHoMObject>();
                        //IEnumerable<object> genericObjs_create = file.Content.Except(bHoMObjects_create);

                        //Diff diffGeneric = BH.Engine.Diffing.Compute.DiffGenericObjects(genericObjs_read, genericObjs_create, null, true);

                        // Then combine the two diffs in one. 
                        // For old objects (= already in file) do not create. 
                        // Create only those that are "new".
                    }
                    else if (pushType == PushType.UpdateOnly || pushType == PushType.UpdateOrCreateOnly) 
                    {
                        // Currently captured by CreateOnly. See above.

                        // For old objects (= already in file) Update Them. 
                        // For those who are "new": create them only if `UpdateOrCreateOnly` is used.
                    }
                    else
                    {
                        BH.Engine.Reflection.Compute.RecordWarning($"The specified Pushtype of {pushType.ToString()} is not supported for .json files.");
                        filecreated = false;
                    }
                }
            }
            catch (Exception e)
            {
                BH.Engine.Reflection.Compute.RecordError(e.Message);
            }

            if (filecreated)
            {
                System.IO.FileInfo fileinfo = new System.IO.FileInfo(fullPath);
                oM.Adapters.File.FSFile createdFile = fileinfo.ToFiling();
                createdFile.Content = file.Content;

                return createdFile;
            }

            BH.Engine.Reflection.Compute.RecordError($"Could not create {file.ToString()}");
            return null;
        }

        /***************************************************/

        public static void WriteJsonFile(string fullPath, string json, bool replaceContent, bool createParentDirectories = true)
        {
            if (createParentDirectories)
            {
                System.IO.FileInfo file = new System.IO.FileInfo(fullPath);
                file.Directory.Create(); // If the directory already exists, this method does nothing.
            }

            if (replaceContent)
                System.IO.File.WriteAllText(fullPath, json); // Replace all content.
            else
                System.IO.File.AppendAllText(fullPath, json);
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static string BeautifyJson(string jsonString)
        {
            JsonDocument doc = JsonDocument.Parse(
                jsonString,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = true
                }
            );
            MemoryStream memoryStream = new MemoryStream();
            using (
                var utf8JsonWriter = new Utf8JsonWriter(
                    memoryStream,
                    new JsonWriterOptions
                    {
                        Indented = true
                    }
                )
            )
            {
                doc.WriteTo(utf8JsonWriter);
            }
            return new System.Text.UTF8Encoding()
                .GetString(memoryStream.ToArray());
        }
    }
}

