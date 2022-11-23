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
    [Description("Used to read contents of one or more files.")]
    public class FileContentRequest : IFilingRequest
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        [Description("The content from this File will be queried.\n" +
            "You can also specify the file with a string Path. ")]
        public virtual FSFile File { get; set; }

        [Description("Only objects of a Type specified in this list will be returned.")]
        public virtual List<Type> Types { get; set; } = new List<Type>();

        [Description("Only BHoMObjects whose CustomData contains these key/value pairs will be returned.")]
        public virtual List<string> CustomDataKeys { get; set; } = new List<string>();

        [Description("Only BHoMObjects that own a Fragment of one of these Types will be returned.")]
        public virtual List<Type> FragmentTypes { get; set; } = new List<Type>();
    }
}


