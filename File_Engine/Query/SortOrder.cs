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

        [Description("Sorts the File-system resources following a given sorting order.")]
        public static List<IFSInfo> SortOrder(this List<IFSInfo> resources, SortOrder sortOrder)
        {
            IEnumerable<IFSInfo> output = resources.ToList(); 

            if (sortOrder == BH.oM.Adapters.File.SortOrder.ByName)
                output = output.OrderBy(x => x.Name);

            if (sortOrder == BH.oM.Adapters.File.SortOrder.BySize)
                output = output.OrderBy(x => x.Size).Reverse();

            if (sortOrder == BH.oM.Adapters.File.SortOrder.ByCreationTime)
                output = output.OrderBy(x => x.CreationTimeUtc).Reverse();

            if (sortOrder == BH.oM.Adapters.File.SortOrder.ByLastModifiedTime)
                output = output.OrderBy(x => x.ModifiedTimeUtc).Reverse();

            return output.ToList();
        }
    }
}


