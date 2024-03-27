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


using BH.oM.Adapter;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.File
{
    public class PushConfig : ActionConfig
    {
        [Description("When serialising to JSON, use the Dataset serialization style." +
            "\nThis serializes the individual objects, and then concatenates the strings separating with a newline." +
            "\nThe obtained format is not valid JSON. You will need to deserialize each individual line." +
            "\nThis is the current standard for Datasets.")]
        public bool UseDatasetSerialization { get; set; } = false;

        [Description("By default, certain types cannot be pushed to Json as root-level objects, for example numbers.\n" +
            "Set this option to `true` to allow skipping those types.")]
        public bool SkipUnsupportedTypes { get; set; } = false;

        [Description("If true, beautify Json files for web display. Works only if UseDatasetSerialization is set to false.")]
        public bool BeautifyJson { get; set; } = true;

        [Description("Keeps the warnings about Deletion off.")]
        public bool DisableWarnings { get; set; } = false;
    }
}




