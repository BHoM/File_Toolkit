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
using BH.oM.Adapter;
using BH.Engine.Adapters.File;
using BH.oM.Adapters.File;
using System.Text.Json;
using BH.Engine.Diffing;
using BH.oM.Diffing;
using BH.oM.Data.Library;

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

            // Process file content, only if there is any.
            if (file.Content != null && file.Content.Count != 0)
            {
                if (file.Content.All(o => o is Dataset))
                    pushConfig.UseDatasetSerialization = true;

                if (!pushConfig.UseDatasetSerialization)
                {
                    allLines.AddRange(file.Content.Where(c => c != null).Select(obj => obj.ToJson() + ","));

                    // Remove the trailing comma if there is only one element.
                    allLines[allLines.Count - 1] = allLines[allLines.Count - 1].Remove(allLines[allLines.Count - 1].Length - 1);

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

                if (pushConfig.BeautifyJson)
                {
                    try
                    {
                        json = BeautifyJson(json);
                    }
                    catch (Exception e)
                    {
                        BH.Engine.Base.Compute.RecordWarning($"Beautify json failed. File will be created with non-beautified json. Error:\n{e.Message}");
                    }
                }
            }

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
                            BH.Engine.Base.Compute.RecordNote($"File {fullPath} was not updated as no file existed at that location.");
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
                            BH.Engine.Base.Compute.RecordNote($"File {fullPath} was not created as it existed already (Pushtype {pushType.ToString()} was specified).");
                    }
                    else
                    {
                        BH.Engine.Base.Compute.RecordWarning($"The specified Pushtype of {pushType.ToString()} is not supported for .json files.");
                        filecreated = false;
                    }
                }
                else // We are talking about File content.
                {
                    if (pushType == PushType.DeleteThenCreate)
                    {
                        BH.Engine.Base.Compute.RecordNote($"Replacing entire content of file `{fullPath}`.");

                        // Replace all content.
                        WriteJsonFile(fullPath, json, true);
                    }
                    else if (pushType == PushType.CreateOnly || pushType == PushType.CreateNonExisting || pushType == PushType.UpdateOnly || pushType == PushType.UpdateOrCreateOnly)
                    {
                        // Should be refactored to cover distinct use cases for CreateNonExisting, UpdateOnly, UpdateOrCreateOnly
                        if (fileExisted)
                            BH.Engine.Base.Compute.RecordNote($"Appending content to file `{fullPath}`.");

                        WriteJsonFile(fullPath, json, false);
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
                        BH.Engine.Base.Compute.RecordWarning($"The specified Pushtype of {pushType.ToString()} is not supported for .json files.");
                        filecreated = false;
                    }
                }
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError(e.Message);
            }

            if (filecreated)
            {
                System.IO.FileInfo fileinfo = new System.IO.FileInfo(fullPath);
                oM.Adapters.File.FSFile createdFile = fileinfo.ToFiling();
                createdFile.Content = file.Content;

                return createdFile;
            }

            BH.Engine.Base.Compute.RecordError($"Could not create {file.ToString()}");
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

            // Treat the "empty input text" case.
            if (string.IsNullOrWhiteSpace(json))
            {
                if (replaceContent)
                    System.IO.File.WriteAllText(fullPath, json); // still write the file
                else
                    System.IO.File.AppendAllText(fullPath, json); // still append empty text (modifying the file)

                return;
            }

            // If replaceContent is true, or file doesn't exist, or file exists but is empty, then replace all content.
            if (replaceContent || !System.IO.File.Exists(fullPath) || !System.IO.File.ReadAllText(fullPath).Any())
                System.IO.File.WriteAllText(fullPath, json);
            else
            {
                // We are appending to an existing, non-empty JSON file.
                // This operation is valid only if the existing json file is a JSON array, and if the text to be appended is a JSON array too.

                // Process the text to be appended
                string inputJson = json;
                if (inputJson.First() == '[' && inputJson.Last() == ']')
                {
                    // The input text is a JSON array. Remove square brackets.
                    inputJson = inputJson.Remove(inputJson.Count() - 1).Remove(0, 1);

                    // Read the Json file
                    string existingJson = System.IO.File.ReadAllText(fullPath);
                    if (existingJson.First() != '[' || existingJson.Last() != ']')
                    {
                        BH.Engine.Base.Compute.RecordError("Invalid operation: attempted to append content to an existing JSON that was not a valid JSON array.");
                        return;
                    }

                    // Replace last ']' with a comma
                    string toBeCreated = existingJson.Remove(existingJson.Count() - 1) + ",";

                    // Append content and close square brackets.
                    toBeCreated += "\n" + inputJson + "]";

                    System.IO.File.WriteAllText(fullPath, toBeCreated);

                    return;
                }


                string[] asd = new string[] { "" };
                if (!string.IsNullOrWhiteSpace(json) && json.Contains("\r\n"))
                    asd = json.Split(new[] { "\r\n" }, StringSplitOptions.None);

                if (asd.Count() > 0 && json.FirstOrDefault() == '{' && json.LastOrDefault() == '}')
                {
                    // The input text is a "bhom dataset" json. 

                    // Read the Json file
                    string existingJson = System.IO.File.ReadAllText(fullPath);
                    if (existingJson.First() != '{' || existingJson.Last() != '}')
                    {
                        BH.Engine.Base.Compute.RecordError("Invalid operation: attempted to append 'BHoM-Dataset' text to an existing .json file that was not a 'BHoM-Dataset' file.");
                        return;
                    }

                    System.IO.File.AppendAllText(fullPath, Environment.NewLine + json);
                }
            }
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static string BeautifyJson(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return jsonString;

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



