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

using BH.Engine.Adapters.File;
using BH.Engine.Reflection;
using BH.oM.Adapter;
using BH.oM.Data.Requests;
using BH.oM.Adapters.File;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        public override bool SetupPullRequest(object request, out IRequest pullRequest)
        {
            pullRequest = request as IRequest;

            // If there is no input request, but a target filepath was specified through the Adapter constructor, use that.
            if (!string.IsNullOrWhiteSpace(m_defaultFilePath) && (request == null || request is Type))
            {
                if (request is Type)
                {
                    pullRequest = new FileContentRequest() { File = m_defaultFilePath, Types = new List<Type>() { request as Type } };
                    BH.Engine.Base.Compute.RecordNote($"Type interpreted as new {nameof(FileContentRequest)} targeting the Adapter targetLocation: `{m_defaultFilePath}` and filtering per type `{(request as Type).Name}`.");
                }
                else if (request is IEnumerable<Type>)
                {
                    pullRequest = new FileContentRequest() { File = m_defaultFilePath, Types = (request as IEnumerable<Type>).ToList() };
                    BH.Engine.Base.Compute.RecordNote($"Type interpreted as new {nameof(FileContentRequest)} targeting the Adapter targetLocation: `{m_defaultFilePath}` and filtering per types: {String.Join(", ",(request as IEnumerable<Type>).Select(t => t.Name))}.");
                }
                else
                {
                    pullRequest = new FileContentRequest() { File = m_defaultFilePath };
                    BH.Engine.Base.Compute.RecordNote($"Pulling file contents from the Adapter targetLocation: `{m_defaultFilePath}`.");
                }

                return true;
            }

            if (request == null && string.IsNullOrWhiteSpace(m_defaultFilePath))
            {
                BH.Engine.Base.Compute.RecordError($"Please specify a valid Request, or create the {nameof(FileAdapter)} with the constructor that takes inputs to specify a target Location.");
                return false;
            }

            if ((request as IRequest) != null && !string.IsNullOrWhiteSpace(m_defaultFilePath))
            {
                BH.Engine.Base.Compute.RecordWarning($"Both request and targetLocation have been specified. Requests take precedence. Pulling as specified by the input `{request.GetType().Name}`.");
                return true;
            }

            return base.SetupPullRequest(request, out pullRequest);
        }

        public override IEnumerable<object> Pull(IRequest request, PullType pullType = PullType.AdapterDefault, ActionConfig actionConfig = null)
        {
            return Read(request as dynamic, actionConfig as PullConfig ?? new PullConfig());
        }

        /***************************************************/

    }
}



