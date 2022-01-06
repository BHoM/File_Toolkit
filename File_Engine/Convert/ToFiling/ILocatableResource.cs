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

using BH.oM.Adapters.File;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Adapters.File
{
    public static partial class Convert
    {
        [Description("Attempts conversion of a generic Resource to a File-system Resource.")]
        public static oM.Adapters.File.IFSContainer ToFiling(this ILocatableResource iLocRes)
        {
            try
            {
                IFSContainer fileOrDir = null;

                if (iLocRes is IFile)
                    fileOrDir = (FSFile)Path.Combine(iLocRes.Location, iLocRes.Name ?? "");

                if (iLocRes is IDirectory)
                    fileOrDir = (FSDirectory)Path.Combine(iLocRes.Location, iLocRes?.Name ?? "");

                IContainableResource iContRes = iLocRes as IContainableResource;
                if (iContRes != null && fileOrDir != null)
                    fileOrDir.Content = iContRes.Content;

                return fileOrDir;
            }
            catch { }

            try
            {
                FSDirectory dir = (FSDirectory)Path.Combine(iLocRes.Location, iLocRes.Name ?? "");
                return dir;
            }
            catch { }

            BH.Engine.Reflection.Compute.RecordError($"The resource {iLocRes.IFullPath()} has an invalid path.");

            return null;
        }
    }
}


