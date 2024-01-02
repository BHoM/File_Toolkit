/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

namespace BH.Engine.Adapters.File
{
    public static partial class Query
    {
        /***************************************************/
        /*** Methods                                     ***/
        /***************************************************/

        [Description("Test whether the file hierarchy is acyclic, i.e. has no loops in it.")]
        [Input("file", "The file to test.")]
        [Output("Whether the file hierachy is Acyclic.")]
        public static bool IsAcyclic(this IFSInfo file)
        {
            return IsAcyclic(file as dynamic, new HashSet<IFSInfo>());
        }

        /***************************************************/

        private static bool IsAcyclic(IFSInfo file, HashSet<IFSInfo> encountered)
        {
            if (file?.ParentDirectory == null) return true;

            if (encountered.Contains(file.ParentDirectory)) return false;

            encountered.Add(file.ParentDirectory);

            return IsAcyclic(file.ParentDirectory, encountered);
        }

        /***************************************************/
    }
}




