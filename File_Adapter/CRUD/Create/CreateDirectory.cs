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
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BH.Engine.Serialiser;
using BH.oM.Adapter;
using BH.Engine.Adapters.File;
using BH.oM.Adapters.File;
using BH.Engine.Base;

namespace BH.Adapter.File
{
    public partial class FileAdapter : BHoMAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public IFSContainer CreateDirectory(FSDirectory dir, PushType pushType, PushConfig pushConfig)
        {
            List<BH.oM.Adapters.File.IFSContainer> createdDirs = new List<oM.Adapters.File.IFSContainer>();

            bool clearfile = pushType == PushType.DeleteThenCreate ? true : false;

            string dirFullPath = dir.IFullPath();
            bool existed = System.IO.Directory.Exists(dirFullPath);

            bool directoryCreated = true;

            try
            {
                if (pushType == PushType.DeleteThenCreate) // Deletes and recreates the directory.
                {
                    if (existed)
                        System.IO.Directory.Delete(dirFullPath, true); // Deletes the directory and all contents. To make things safer, a Warning is exposed in the Push before proceeding.

                    System.IO.Directory.CreateDirectory(dirFullPath);
                }
                else if (pushType == PushType.CreateOnly || pushType == PushType.CreateNonExisting || pushType == PushType.UpdateOrCreateOnly || pushType == PushType.UpdateOnly)
                {
                    // Create only directories that didn't exist.
                    if (pushType != PushType.UpdateOnly)
                    {
                        if (!existed)
                            System.IO.Directory.CreateDirectory(dirFullPath);
                        else
                        {
                            BH.Engine.Base.Compute.RecordNote($"Directory {dirFullPath} was not created as it existed already (Pushtype {pushType.ToString()} was specified).");
                            directoryCreated = false;
                        }
                    }

                    if (dir.Content != null && dir.Content.Any())
                        for (int i = 0; i < dir.Content.Count; i++)
                        {
                            ILocatableResource item = (dir.Content[i] as ILocatableResource).DeepClone();
                            if (item == null)
                                BH.Engine.Base.Compute.RecordWarning($"Cannot push Directory content {dir.Content[i].GetType().Name}.");

                            string itemFullPath = item.IFullPath();
                            if (string.IsNullOrWhiteSpace(itemFullPath) && !string.IsNullOrWhiteSpace(item.Name))
                            {
                                itemFullPath = Path.Combine(dirFullPath, item.Name); // Default to Container Directory path.
                                item.Location = Path.GetDirectoryName(itemFullPath);
                            }

                            if (item.Location == dirFullPath)
                                Create(item, pushType, pushConfig);
                            else
                            {
                                BH.Engine.Base.Compute.RecordWarning($"The content of the Directory {dirFullPath} can't be Pushed because the content Path {itemFullPath} does not match the container Directory path.");
                            }
                        }
                }
                else
                {
                    BH.Engine.Base.Compute.RecordWarning($"The specified Pushtype of {pushType.ToString()} is not supported for {nameof(BH.oM.Adapters.File.FSDirectory)} objects.");
                    directoryCreated = false;
                }
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError(e.Message);
            }

            if (directoryCreated || existed)
            {
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(dirFullPath);
                oM.Adapters.File.FSDirectory createdDir = dirInfo.ToFiling();

                return createdDir;
            }

            BH.Engine.Base.Compute.RecordError($"Could not create the Directory {dir.ToString()}.");
            return null;
        }
    }

}



