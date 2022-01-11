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

        [Description("Tells if a filename or path contains a Regex. Also parses for valid BHoM-regex operators, like the single asterisk `*`.")]
        public static bool TryGetRegexFromPath(string fullPath, out Regex regex)
        {
            string regexStr = fullPath;
            regex = null;

            try
            {
                // If the path is a valid Uri, it may still contain a Query (e.g. "http://www.somesite.com/folder/file.jpg?q=randomquery.string")
                // that gives a false positive for a Regex
                Uri uriRemovedQuery = new Uri(regexStr);
                if (!uriRemovedQuery.IsFile)
                    regexStr = String.Format("{0}{1}{2}{3}", uriRemovedQuery.Scheme, Uri.SchemeDelimiter, uriRemovedQuery.Authority, uriRemovedQuery.AbsolutePath);
            }
            catch { }
            
            // First, if there is a regex text, it must be in the last part of the Path. We can get it with GetFileName.
            regexStr = Path.GetFileName(regexStr);

            // We then need to check if the path was pointing to a single file,
            // in which case we need to avoid the confusion between the `.` in the extension and a Regex dot operator.
            int regexOperatorsFound = m_regexOperatorChars.Count(c => regexStr.Contains(c));

            // If there are no Regex operator, this points to a single File or Directory. Do not return any regex.
            if (regexOperatorsFound == 0)
                return false;

            int dotFound = regexStr.Count(f => f == '.');

            if (regexOperatorsFound == 1 && dotFound == 1)
            {
                // If there is only one dot in the string, it might be representing the extension.
                // We need to check whether the char before and/or after it is a Regex operator.
                char charBeforeDot = regexStr.ElementAtOrDefault(regexStr.IndexOf('.') - 1);
                char charAfterDot = regexStr.ElementAtOrDefault(regexStr.IndexOf('.') + 1);

                // We can now confirm whether the dot is just representing an extension, 
                // in which case we are not returning any Regex.
                if (Path.HasExtension(fullPath) && !m_regexOperatorChars.Contains(charBeforeDot) && !m_regexOperatorChars.Contains(charAfterDot))
                    return false;
            }

            if(regexStr.EndsWith(".*"))
            {
                // We need to assume that the user wanted to say "any extension" rather than "any character".
                // "Any character at the end of the string" can therefore only be obtained by specifying a single asterisk at the end (with no previous dot).
                regexStr = "^" + Regex.Escape(regexStr).Replace("\\*", ".*") + "$";
            }

            regexStr = WildcardsToRegex(regexStr);

            try
            {
                regex = new Regex(regexStr);
            }
            catch (Exception e)
            {
                // Invalid regex
                return false;
            }

            return true;
        }

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

        private static List<char> m_regexOperatorChars = new List<char> { '.', '/', '\\', '[', ']', '$', '*', '^', '+', '?', '{', '}', '(', ')', '/' };


        /***************************************************/
    }
}


