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

using System;
using System.Security.AccessControl;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BH.oM.Adapters.File;
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Adapter.File
{
    public partial class FileAdapter
    {
        /*******************************************/
        /**** Methods                           ****/
        /*******************************************/

        [Input("parentDirectory","Path of parent Directory of the file. You can also specify a string path.")]
        [Input("fullFileName", "Name of the file, INCLUDING Extension.")]
        [Input("content", "The content of the file.")]
        [Description("Creates a FileSystem-hosted object.")]
        public static FSFile CreateFSFile(FSDirectory parentDirectory, string fullFileName, List<object> content = null)
        {
            if (!Path.HasExtension(fullFileName))
            {
                BH.Engine.Base.Compute.RecordError($"Please include the extension in the {nameof(fullFileName)}.");
                return null;
            }

            return new oM.Adapters.File.FSFile()
            {
                ParentDirectory = parentDirectory,
                Name = fullFileName,
                Content = content
            };
        }

        /*******************************************/
    }
}


