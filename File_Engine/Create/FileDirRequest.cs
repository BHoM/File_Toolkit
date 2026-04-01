/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using System;
using System.Security.AccessControl;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BH.oM.Adapters.File;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.File
{
    public static partial class Create
    {
        /*******************************************/
        /**** Methods                           ****/
        /*******************************************/

        [Description("Creates a FileDirRequest for querying files and/or directories at the specified location.")]
        [Input("fullPath", "The full path to the directory to query.")]
        [Input("includeDirectories", "Whether to include directories in the results.")]
        [Input("includeFiles", "Whether to include files in the results.")]
        [Input("searchSubdirectories", "Whether to search recursively in subdirectories.")]
        [Input("includeFileContents", "Whether to include the contents of the files in the results.")]
        [Output("fileDirRequest", "A FileDirRequest configured with the provided options.")]
        public static FileDirRequest FileDirRequest(string fullPath, bool includeDirectories = true, bool includeFiles = true, bool searchSubdirectories = true, bool includeFileContents = false)
        {
            return new FileDirRequest()
            {
                Location = fullPath,
                IncludeDirectories = includeDirectories,
                IncludeFiles = includeFiles,
                SearchSubdirectories = searchSubdirectories,
                IncludeFileContents = includeFileContents
            };
        }

        /*******************************************/
    }
}






