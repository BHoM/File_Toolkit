/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BH.Engine.Adapters.File
{
    public static partial class Query
    {
        /***************************************************/
        /*** Methods                                     ***/
        /***************************************************/

        [Description("Replaces wildcards (such as '*') in the input string, in order to form a proper Regex string.")]
        public static string WildcardsToRegex(this string str)
        {
            // Parse for asterisks
            for (int i = 0; i < str.Count(); i++)
            {
                if (str[i] != '*')
                    continue;

                // We must check if the asterisk is preceded by another regex operator.
                char charBeforeAsterisk = str.ElementAtOrDefault(i - 1);

                // If not, then the user intended to use it as a wildcard alone.
                if (!m_regexOperatorChars.Contains(charBeforeAsterisk))
                    str = "^" + Regex.Escape(str).Replace("\\*", ".*") + "$"; //Converts the asterisks into a Regex. https://stackoverflow.com/a/30300521/3873799
            }

            return str;
        }


        /***************************************************/
    }
}




