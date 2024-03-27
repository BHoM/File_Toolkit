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
    public static partial class Modify
    {
        /***************************************************/
        /*** Methods                                     ***/
        /***************************************************/

        [Description("Rename a file or directory.")]
        [Input("fileOrDir", "The file or directory to rename.")]
        [Input("name", "The new name.")]
        [Output("The moved file object.")]
        [PreviousVersion("7.1", "BH.Engine.Adapters.File.Modify.Rename(BH.oM.Adapters.File.FSFile, System.String)")]
        [PreviousVersion("7.1", "BH.Engine.Adapters.File.Modify.Rename(BH.oM.Adapters.File.FSDirectory, System.String)")]
        public static IFSContainer IRename(this oM.Adapters.File.IFSContainer fileOrDir, string name)
        {
            fileOrDir = BH.Engine.Base.Query.ShallowClone(fileOrDir);
            return Rename(fileOrDir as dynamic, name);
        }

        /***************************************************/
        /**** Private interface methods                 ****/
        /***************************************************/

        private static IFSContainer Rename(this FSFile file, string name)
        {
            file.Name = name;
            return file;
        }

        /***************************************************/

        private static IFSContainer Rename(this FSDirectory directory, string name)
        {
            directory.Name = name;
            return directory;
        }

        /***************************************************/
        /**** Private interface methods - fallback      ****/
        /***************************************************/

        private static IFSContainer Rename(this IFSContainer container, string name)
        {
            BH.Engine.Base.Compute.RecordError($"Cannot rename FSContainers of type: {container.GetType().FullName}, please provide either an FSFile or FSDirectory object.");
            return null;
        }

        /***************************************************/
    }
}




