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

using BH.oM.Adapters.File;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Adapters.File
{
    public static partial class Modify
    {
        /***************************************************/
        /*** Methods                                     ***/
        /***************************************************/

        [Description("Get the contents of the file as a string, reading from its location.")]
        [Input("fileDirectoryRequest", "The file directory request to process.")]
        [Output("success", "True if the wildcard pattern is valid.")]
        public static bool ProcessFileDirRequest(this FileDirRequest fileDirectoryRequest, out WildcardPattern wildcardPattern)
        {
            wildcardPattern = null;

            if (WildcardPattern.ContainsWildcardCharacters(fileDirectoryRequest.Location))
            {
                if (WildcardPattern.ContainsWildcardCharacters(fileDirectoryRequest.Location))
                {
                    string allButLastSegment = fileDirectoryRequest.Location.Remove(fileDirectoryRequest.Location.Count() - Path.GetFileName(fileDirectoryRequest.Location).Count());
                    if (WildcardPattern.ContainsWildcardCharacters(allButLastSegment))
                    {
                        BH.Engine.Base.Compute.RecordError("Wildcards are only allowed in the last segment of the path.");
                        return false;
                    }
                    else
                        wildcardPattern = new WildcardPattern(Path.GetFileName(fileDirectoryRequest.Location));
                }

                if (fileDirectoryRequest.IncludeDirectories)
                {
                    BH.Engine.Base.Compute.RecordWarning($"The usage of Wildcards is limited to file retrievals: " +
                        $"\ncannot have `{nameof(FileDirRequest)}.{nameof(fileDirectoryRequest.IncludeDirectories)}` set to true while a Wildcard is specified in the path." +
                        $"\nDefaulting `{nameof(fileDirectoryRequest.IncludeDirectories)}` to false and continuing.");
                    fileDirectoryRequest.IncludeDirectories = false;
                }

                if (fileDirectoryRequest.IncludeFileContents)
                    BH.Engine.Base.Compute.RecordNote($"Note that `{nameof(fileDirectoryRequest.IncludeFileContents)}` works only for BHoM-serialized JSON files.");
            }

            return true;
        }

        /***************************************************/
    }
}





