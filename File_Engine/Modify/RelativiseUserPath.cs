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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.Base;
using BH.Engine.Reflection;
using System.IO;

namespace BH.Engine.Adapters.File
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Methods                                   ****/
        /***************************************************/

        [Description("If an user path is specified – e.g. containing `C:\\Users\\SomeUser` – modifies SomeUser to match the current user.")]
        public static string RelativiseUserPath(string path)
        {
            string relativisedPath = path;
            char separator = '\\';

            string normalisedPath = path.NormalisePath(false);
            string[] directories = normalisedPath.Split(separator);

            string currentUserPath = GetUserPath();
            string currentUserFolderName = currentUserPath.Split(separator).Last();

            if (normalisedPath.StartsWith(Path.GetDirectoryName(currentUserPath)) && directories.Length > 2)
            {
                directories[2] = currentUserFolderName;

                relativisedPath = Path.Combine(directories);

                relativisedPath = string.Join(separator.ToString(), directories);
            }
            return relativisedPath;
        }

        // Get the user folder, in the form of C:\Users\UserName.
        // Written to work also for Windows versions prior to 10.
        private static string GetUserPath()
        {
            string path = System.IO.Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                path = System.IO.Directory.GetParent(path).ToString();
            }

            return path;
        }
    }
}


