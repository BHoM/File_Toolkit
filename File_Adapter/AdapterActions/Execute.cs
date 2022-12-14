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
using BH.oM.Base.Attributes;

namespace BH.Adapter.File
{
    public partial class FileAdapter
    {
        /***************************************************/
        /**** Methods                                  *****/
        /***************************************************/

        [MultiOutput(0, "success", "List of booleans indicating whether the command succeeded for the individual items.")]
        [MultiOutput(1, "GlobalSuccess", "Bool indicating whether the command succeded for all the provided inputs.")]
        public override Output<List<object>, bool> Execute(IExecuteCommand command, ActionConfig actionConfig = null)
        {
            if (command == null)
                return new Output<List<object>, bool>();

            oM.Adapters.File.ExecuteConfig executeConfig = actionConfig as oM.Adapters.File.ExecuteConfig ?? new ExecuteConfig();

            if (m_Execute_enableWarning && !executeConfig.DisableWarnings)
            {
                BH.Engine.Base.Compute.RecordWarning($"This Action can move, rename and copy files and folders with their contents." +
                    $"\nMake sure that you know what you are doing. This warning will not be repeated." +
                    $"\nRe-enable the component to continue.");

                m_Execute_enableWarning = false;

                return new Output<List<object>, bool>();
            }

            var output = RunCommand(command as dynamic);

            return output;
        }

        /***************************************************/

        private Output<List<object>, bool> RunCommand(IMRCCommand command)
        {
            string source = command.FullPath?.NormalisePath(false);
            string target = command.TargetFullPath?.NormalisePath(false);
            bool overwriteTarget = command.OverwriteTarget;

            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
            {
                BH.Engine.Base.Compute.RecordWarning("Please specify a valid oM.Adapters.Filing.Command.");
                return null;
            }
            RenameCommand renameCommand = command as RenameCommand;
            if (renameCommand != null)
                return Move(source, target, true, overwriteTarget);

            MoveCommand moveCommand = command as MoveCommand;
            if (moveCommand != null)
                return Move(source, target, false, overwriteTarget, moveCommand.CreateDirectoryIfNotExist);

            CopyCommand copyCommand = command as CopyCommand;
            if (copyCommand != null)
                return Copy(source, target, overwriteTarget, copyCommand.CreateDirectoryIfNotExist);

            return new Output<List<object>, bool>() { Item1 = new List<object>(), Item2 = false };
        }

        private Output<List<object>, bool> Move(string source, string target, bool renameOnlyWithinFolder, bool overwriteTarget = false, bool createDir = true)
        {
            var output = new Output<List<object>, bool>() { Item1 = new List<object>(), Item2 = false };

            string parent1 = System.IO.Directory.GetParent(source).FullName;
            string parent2 = System.IO.Directory.GetParent(target).FullName;

            if (renameOnlyWithinFolder && parent1 != parent2)
            {
                BH.Engine.Base.Compute.RecordWarning($"Cannot rename `{source}`" +
                    $"because the target parent folder is different from the original.");

                output.Item2 = false;

                return output;
            }

            try
            {
                if (overwriteTarget && System.IO.File.Exists(target))
                {
                    System.IO.File.Delete(target);
                }

                if (createDir && !System.IO.Directory.Exists(parent2))
                    System.IO.Directory.CreateDirectory(parent2);

                System.IO.File.Move(source, target);
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordWarning(e.Message);
                return output;
            }

            output.Item1.Add(target);
            output.Item2 = true;

            return output;
        }

        private Output<List<object>, bool> Copy(string source, string target, bool overwrite = false, bool createDir = true)
        {
            var output = new Output<List<object>, bool>() { Item1 = new List<object>(), Item2 = false };

            try
            {
                if (createDir)
                    (new FileInfo(target)).Directory.Create(); // Creates the directory if it doesn't exist.

                System.IO.File.Copy(source, target, overwrite);
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordWarning(e.Message);
                return output;
            }

            output.Item1.Add(target);
            output.Item2 = true;

            return output;
        }
    }
}


