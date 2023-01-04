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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Adapters.File
{
    public static partial class Compute
    {
        [Description("If the targetPath points to a file that exists, return the same filepath with appended `- Copy (i)`, " +
            "where `i` is the first index pointing to a non existing file.")]
        public static string NewFilenameIfExists(string targetPath)
        {
            int i = 0;
            do
            {
                string copyFileName = Path.GetFileNameWithoutExtension(targetPath);

                string copyStr = $" - Copy";

                if (copyFileName.Substring(copyFileName.Length - copyStr.Length - 4).Contains($" - Copy ("))
                {
                    copyFileName = copyFileName.Remove(copyFileName.Length - copyStr.Length - 4) + $" - Copy ({i})";
                }

                if (copyFileName.EndsWith(copyStr))
                    copyFileName = copyFileName.Replace(copyStr, $" - Copy ({i})");

                if (copyFileName.EndsWith(" - Copy (0)"))
                    copyFileName.Replace(" - Copy (0)", copyStr);

                if (!copyFileName.Substring(copyFileName.Length - copyStr.Length - 4).Contains($" - Copy"))
                {
                    copyFileName += $" - Copy";
                }

                targetPath = Path.Combine(Path.GetDirectoryName(targetPath), copyFileName + Path.GetExtension(targetPath));
                i++;
            }
            while (System.IO.File.Exists(targetPath));

            return targetPath;
        }
    }
}



