/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
        [Input("file", "The file to get the contents of.")]
        [Input("encoding", "The encoding to use to decode the data. If null (default) discovery will be attempted; defaults to UTF-8 if it can't be discovered.")]
        [Output("The contents of the file.")]
        public static bool ProcessFileDirRequest(this FileDirRequest fdr, out WildcardPattern wildcardPattern)
        {
            wildcardPattern = null;

            if (WildcardPattern.ContainsWildcardCharacters(fdr.Location))
            {
                if (WildcardPattern.ContainsWildcardCharacters(fdr.Location))
                {
                    string allButLastSegment = fdr.Location.Remove(fdr.Location.Count() - Path.GetFileName(fdr.Location).Count());
                    if (WildcardPattern.ContainsWildcardCharacters(allButLastSegment))
                    {
                        BH.Engine.Base.Compute.RecordError("Wildcards are only allowed in the last segment of the path.");
                        return false;
                    }
                    else
                        wildcardPattern = new WildcardPattern(Path.GetFileName(fdr.Location));
                }

                if (fdr.IncludeDirectories)
                {
                    BH.Engine.Base.Compute.RecordWarning($"The usage of Wildcards is limited to file retrievals: " +
                        $"\ncannot have `{nameof(FileDirRequest)}.{nameof(fdr.IncludeDirectories)}` set to true while a Wildcard is specified in the path." +
                        $"\nDefaulting `{nameof(fdr.IncludeDirectories)}` to false and continuing.");
                    fdr.IncludeDirectories = false;
                }

                if (fdr.IncludeFileContents)
                    BH.Engine.Base.Compute.RecordNote($"Note that `{nameof(fdr.IncludeFileContents)}` works only for BHoM-serialized JSON files.");
            }

            return true;
        }

        /***************************************************/
    }
}



