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

using BH.Engine.Adapters.File;
using BH.Engine.Reflection;
using BH.oM.Adapter;
using BH.oM.Data.Requests;
using BH.oM.Adapters.File;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.Base;

namespace BH.Adapter.File
{
    public partial class FileAdapter
    {
        /***************************************************/
        /**** Methods                                  *****/
        /***************************************************/

        public override int Remove(IRequest request, ActionConfig actionConfig = null)
        {
            RemoveRequest removeRequest = request as RemoveRequest;
            if (removeRequest == null)
            {
                BH.Engine.Base.Compute.RecordWarning($"Please specify a valid {nameof(RemoveRequest)}.");
                return 0;
            }

            oM.Adapters.File.RemoveConfig removeConfig = actionConfig as oM.Adapters.File.RemoveConfig ?? new RemoveConfig();

            if (m_Remove_enableDeleteWarning && !removeConfig.DisableWarnings)
            {
                BH.Engine.Base.Compute.RecordWarning($"This Action can delete files and folders with their contents." +
                    $"\nMake sure that you know what you are doing. Re-enable the component to continue.");

                m_Remove_enableDeleteWarning = false;

                return 0;
            }

            if (m_Remove_enableDeleteAlert && (removeConfig.IncludeHiddenFiles))
            {
                BH.Engine.Base.Compute.RecordError($"Your {nameof(removeConfig)} is set to {nameof(removeConfig.IncludeHiddenFiles)}" +
                    $"\nMake sure you know what you are doing. To continue, re-enable the component.");

                m_Remove_enableDeleteAlert = false;

                return 0;
            }


            int deletedCount = Delete(request as dynamic, removeConfig);

            if (deletedCount > 0)
            {
                m_Remove_enableDeleteWarning = true; // re-enable the warning if any object was successfully deleted.
            }

            m_Remove_enableDeleteAlert = true; // always re-enable the alert for hidden files.

            return deletedCount;
        }

        /***************************************************/
    }
}


