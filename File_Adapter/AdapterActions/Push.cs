/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.Engine.Adapters.File;
using BH.Engine.Reflection;
using BH.oM.Adapter;
using BH.oM.Data.Requests;
using BH.oM.Adapters.File;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.Base;
using BH.oM.Base;
using BH.oM.Data.Library;

namespace BH.Adapter.File
{
    public partial class FileAdapter
    {
        /***************************************************/
        /**** Methods                                  *****/
        /***************************************************/

        public override bool SetupPushType(PushType pushType, out PushType pt)
        {
            pt = pushType;

            if (pushType == PushType.AdapterDefault)
                pt = m_AdapterSettings.DefaultPushType;

            if (pushType == PushType.FullPush)
            {
                BH.Engine.Base.Compute.RecordError($"The specified {nameof(PushType)} {nameof(PushType.FullPush)} is not supported.");
                return false;
            }

            return true;
        }

        public override bool SetupPushConfig(ActionConfig actionConfig, out ActionConfig pushConfig)
        {
            PushConfig pushCfg = actionConfig as PushConfig ?? new PushConfig();
            pushConfig = pushCfg;

            if (pushCfg.BeautifyJson && pushCfg.UseDatasetSerialization)
            {
                BH.Engine.Base.Compute.RecordError($"Input `{nameof(PushConfig.BeautifyJson)}` and `{nameof(PushConfig.UseDatasetSerialization)}` cannot be both set to True.");
                return false;
            }

            return true;
        }

        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            PushConfig pushConfig = actionConfig as PushConfig == null ? new PushConfig() : actionConfig as PushConfig;

            if (string.IsNullOrWhiteSpace(m_defaultFilePath)) // = if we are about to push multiple files/directories
                if (pushType == PushType.DeleteThenCreate && m_Push_enableDeleteWarning && !pushConfig.DisableWarnings ) 
                {
                    BH.Engine.Base.Compute.RecordWarning($"You have selected the {nameof(PushType)} {nameof(PushType.DeleteThenCreate)}." +
                        $"\nThis has the potential of deleting files and folders with their contents." +
                        $"\nMake sure that you know what you are doing. This warning will not be repeated." +
                        $"\nRe-enable the component to continue.");

                    m_Push_enableDeleteWarning = false;

                    return new List<object>();
                }

            List<IResource> createdFiles = new List<IResource>();

            List<IResource> filesOrDirs = objects.OfType<IResource>().Select(fd => fd.ShallowClone()).ToList();
            List<object> remainder = objects.Where(o => !(o is IResource)).ToList();

            if (remainder.Any())
            {
                if (filesOrDirs.Any())
                {
                    BH.Engine.Base.Compute.RecordError($"Input objects are both of type `{nameof(BH.oM.Adapters.File.File)}`/`{nameof(BH.oM.Adapters.File.Directory)}` and generic objects." +
                      $"\nIn order to push them:" +
                      $"\n\t- for the `{nameof(BH.oM.Adapters.File.File)}`/`{nameof(BH.oM.Adapters.File.Directory)}` objects, use a Push using a {nameof(FileAdapter)} with no targetLocation input;"+
                      $"\n\t- for the generic objects, use a Push using a {nameof(FileAdapter)} that specifies a targetLocation.");
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(m_defaultFilePath))
                {
                    BH.Engine.Base.Compute.RecordError($"To Push objects that are not of type `{nameof(BH.oM.Adapters.File.File)}` or `{nameof(BH.oM.Adapters.File.Directory)}`," +
                        $"\nyou need to specify a target Location by creating the {nameof(FileAdapter)} through the constructor with inputs.");
                    return null;
                }
            }

            if (m_defaultFilePath != null)
            {
                if (filesOrDirs.Any())
                    BH.Engine.Base.Compute.RecordWarning($"A `targetLocation` has been specified in the File_Adapter constructor." +
                        $"\nObjects of type `{nameof(BH.oM.Adapters.File.File)}` or `{nameof(BH.oM.Adapters.File.Directory)}` will be appended to the file at `targetLocation`." +
                        $"\nIf you want to target multiple files, you need create the {nameof(FileAdapter)} through the constructor without inputs.");
            }

            foreach (IResource fileOrDir in filesOrDirs)
            {
                if (fileOrDir == null)
                    continue;

                if (fileOrDir is IFile)
                {
                    string extension = Path.GetExtension(fileOrDir.IFullPath());

                    if (string.IsNullOrWhiteSpace(extension))
                    {
                        BH.Engine.Base.Compute.RecordNote($"File {fileOrDir.IFullPath()} has no extension specified. Defaults to JSON.");
                        extension = ".json";
                        fileOrDir.Name += extension;
                    }

                    if (extension != ".json")
                    {
                        BH.Engine.Base.Compute.RecordWarning($"Cannot create File {fileOrDir.IFullPath()}. Currently only JSON extension is supported.");
                        continue;
                    }
                }

                IResource created = Create(fileOrDir as dynamic, pushType, pushConfig);
                createdFiles.Add(created);
            }

            if (remainder.Any())
            {
                string defaultDirectory = Path.GetDirectoryName(m_defaultFilePath);
                string defaultFileName = Path.GetFileName(m_defaultFilePath);

                FSFile file = CreateFSFile(defaultDirectory, defaultFileName, remainder);

                if (remainder.All(o => o is Dataset))
                {
                    // Automatically set UseDatasetSerialization to true.
                    pushConfig.UseDatasetSerialization = true;
                    pushConfig.BeautifyJson = false;
                }

                IResource created = Create(file, pushType, pushConfig);

                if (created != null)
                    createdFiles.Add(created);
            }

            return createdFiles.OfType<object>().ToList();
        }

        /***************************************************/

    }
}




