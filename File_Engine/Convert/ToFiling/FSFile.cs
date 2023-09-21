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
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Adapters.File
{
    public static partial class Convert
    {
        /***************************************************/
        /*** Methods                                     ***/
        /***************************************************/

        [Description("Converts the provided FileInfo into a BH.oM.Adapters.Filing.File." +
            "\nTo populate its `Content` property you need to pull the file.")]
        public static oM.Adapters.File.FSFile ToFiling(this FileInfo fi)
        {
            if (fi == null) return null;

            oM.Adapters.File.FSFile bf = new oM.Adapters.File.FSFile();

            bf.ParentDirectory = fi.Directory.ToFiling();
            bf.Name = fi.Name;
            bf.Exists = fi.Exists;
            bf.IsReadOnly = fi.IsReadOnly;
            try
            {
                // If the file's attributes are corrupted, this may throw a FileNotFoundException.
                // This was added as part of #168.
                bf.Size = (int)(fi.Length & 0xFFFFFFFF);
            } catch {}

            bf.Attributes = fi.Attributes;
            bf.CreationTimeUtc = fi.CreationTimeUtc;
            bf.ModifiedTimeUtc = fi.LastWriteTimeUtc;

            return bf;
        }

        /***************************************************/
    }
}



