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

using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using BH.oM.Data.Requests;

namespace BH.oM.Adapters.File
{
    [Description("Used to query Directories or Files.")]
    public class DirectoryRequest : IDirectoryRequest
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        [Description("Directory and/or Files from this Location will be queried.")]
        public virtual string Location { get; set; } = "";

        [Description("If enabled, look also in subdirectories.")]
        public virtual bool SearchSubdirectories { get; set; } = false;

        [Description("If IncludeSubdirectories is true, this sets the maximum subdirectiory nesting level to look in." +
            "\nDefaults to -1 which corresponds to no limit.")]
        public virtual int MaxNesting { get; set; } = -1;

        [Description("Sorting order of the extracted Directories.")]
        public SortOrder SortOrder { get; set; }

        [Description("Sets the maximum number of Directories to retrieve, useful when using IncludeSubdirectories." +
            "\nDefaults to -1 which corresponds to no limit.")]
        public virtual int MaxDirectories { get; set; } = -1;

        [Description("These directories will be excluded from the results." +
         "\nYou can also specify string Full Paths.")]
        public virtual List<object> Exclusions { get; set; } = new List<object>();

        /***************************************************/
        /**** Implicit cast                             ****/
        /***************************************************/

        public static implicit operator DirectoryRequest(string fullPath)
        {
            if (!String.IsNullOrWhiteSpace(fullPath))
                return new DirectoryRequest() { Location = fullPath };
            else
                return null;
        }
    }
}


