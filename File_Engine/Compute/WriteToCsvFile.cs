/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.Engine.Serialiser;
using BH.oM.Adapter;
using BH.oM.Adapters.File;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BH.Engine.Adapters.File
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Write a JSON-serialised file with the input data or objects.")]
        [Input("objects", "Objects to write to the file.")]
        [Input("filePath", "Path to the file.")]
        [Input("replace", "If the file exists, you need to set this to true in order to allow overwriting it.")]
        [Input("active", "Boolean used to trigger the function.")]
        public static bool WriteToCsvFile(List<object> lines, string filePath, CsvSettings settings = null, bool replace = false, bool active = false)
        {
            if (!active || lines == null)
                return false;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                BH.Engine.Base.Compute.RecordError($"The filePath `{filePath}` must not be empty.");
                return false;
            }

            // Make sure no invalid chars are present.
            filePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));

            // If the file exists already, stop execution if `replace` is not true.
            bool fileExisted = System.IO.File.Exists(filePath);
            if (!replace && fileExisted)
            {
                BH.Engine.Base.Compute.RecordWarning($"The file `{filePath}` exists already. To replace its content, set `{nameof(replace)}` to true.");
                return false;
            }

            // Serialise to json and create the file and directory.
            string table = From2DArray(lines);
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
            fileInfo.Directory.Create(); // If the directory already exists, this method does nothing.

            try
            {
                System.IO.File.WriteAllText(filePath, table);
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError($"Error writing to file:\n\t{e.ToString()}");
                return false;
            }

            return true;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        // Convert a list of objects to a CSV text string, taking care of the following types: List<List<string>>, List<string>, string[,], string[][], string[]
        public static string From2DArray(this object obj, CsvSettings settings = null)
        {
            if (settings == null)
                settings = new CsvSettings();

            var delim = settings.Delimiter ?? "\t";

            // Normalize to list of rows (each row = string[])
            var rows = new List<string[]>();

            switch (obj)
            {
                // Rectangular array
                case string[,] rect:
                    {
                        int r = rect.GetLength(0);
                        int c = rect.GetLength(1);
                        for (int i = 0; i < r; i++)
                        {
                            var row = new string[c];
                            for (int j = 0; j < c; j++)
                                row[j] = rect[i, j] ?? string.Empty;
                            rows.Add(row);
                        }
                        break;
                    }

                // Jagged array
                case string[][] jagged:
                    {
                        foreach (var row in jagged)
                            rows.Add((row ?? Array.Empty<string>()).Select(v => v ?? string.Empty).ToArray());
                        break;
                    }

                // Any "sequence of sequences of string" (List<List<string>>, IEnumerable<string[]>, etc.)
                case IEnumerable<IEnumerable<string>> seqOfSeq:
                    {
                        foreach (var inner in seqOfSeq)
                            rows.Add((inner ?? Enumerable.Empty<string>()).Select(v => v ?? string.Empty).ToArray());
                        break;
                    }

                // Any "sequence of string" (treated as a single row); exclude string itself
                case IEnumerable<string> seq when !(obj is string):
                    {
                        rows.Add(seq.Select(v => v ?? string.Empty).ToArray());
                        break;
                    }

                // Single string cell
                case string s:
                    {
                        rows.Add(new[] { s });
                        break;
                    }

                // Fallback: single cell ToString()
                default:
                    {
                        rows.Add(new[] { obj?.ToString() ?? string.Empty });
                        break;
                    }
            }

            if (rows.Count == 0)
                return string.Empty;

            // Pad ragged rows
            int maxCols = rows.Max(r => r == null ? 0 : r.Length);
            if (maxCols == 0) maxCols = 1;

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i] ?? Array.Empty<string>();
                if (r.Length < maxCols)
                    rows[i] = r.Concat(Enumerable.Repeat(string.Empty, maxCols - r.Length)).ToArray();
            }

            // Optional index column (1-based)
            if (settings.IncludeIndex)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var newRow = new string[rows[i].Length + 1];
                    newRow[0] = (i + 1).ToString(CultureInfo.InvariantCulture);
                    Array.Copy(rows[i], 0, newRow, 1, rows[i].Length);
                    rows[i] = newRow;
                }
            }

            // Numeric rounding (if requested)
            if (settings.Digit.HasValue)
            {
                int digits = settings.Digit.Value;
                var format = digits > 0 ? "0." + new string('#', digits) : "0";

                for (int i = 0; i < rows.Count; i++)
                {
                    for (int j = 0; j < rows[i].Length; j++)
                    {
                        var v = rows[i][j];
                        double num;
                        if (TryParseNumber(v, out num))
                        {
                            rows[i][j] = Math.Round(num, digits).ToString(format, CultureInfo.InvariantCulture);
                        }
                    }
                }
            }

            // Serialize with escaping
            var sb = new StringBuilder(rows.Count * maxCols * 4);
            for (int i = 0; i < rows.Count; i++)
            {
                var encoded = rows[i].Select(cell => EscapeCsv(cell ?? string.Empty, delim));
                sb.Append(string.Join(delim, encoded));
                if (i < rows.Count - 1)
                    sb.Append('\n');
            }

            return sb.ToString();
        }


        // --- Helpers ---

        private static bool TryParseNumber(string s, out double value)
        {
            // Try strict parse with InvariantCulture; also try trimming spaces.
            if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value))
                return true;

            if (double.TryParse((s ?? string.Empty).Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value))
                return true;

            // Optional: try replacing comma with dot if it looks like a localized decimal
            var swapped = (s ?? string.Empty).Replace(',', '.');
            return double.TryParse(swapped, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
        }

        private static string EscapeCsv(string input, string delimiter)
        {
            if (input == null) return string.Empty;

            bool mustQuote =
                input.Contains('"') ||
                input.Contains('\n') ||
                input.Contains('\r') ||
                (!string.IsNullOrEmpty(delimiter) && input.Contains(delimiter));

            if (!mustQuote)
                return input;

            // Double quotes inside quoted field
            var doubled = input.Replace("\"", "\"\"");
            return $"\"{doubled}\"";
        }


        private static bool IsEnumerableOfEnumerableOfString(object x, out IEnumerable<IEnumerable<string>> result)
        {
            result = null;

            if (x == null || x is string) return false;

            var outer = x as IEnumerable;
            if (outer == null) return false;

            var rows = new List<IEnumerable<string>>();

            foreach (var inner in outer)
            {
                if (!IsEnumerableOfString(inner, out var row))
                    return false; // any inner not IEnumerable<string> -> fail
                rows.Add(row);
            }

            result = rows;
            return true;
        }

        private static bool IsEnumerableOfString(object x, out IEnumerable<string> result)
        {
            result = null;

            // Exclude string itself (it is IEnumerable<char>)
            if (x == null || x is string) return false;

            var enumerable = x as IEnumerable;
            if (enumerable == null) return false;

            var list = new List<string>();
            foreach (var item in enumerable)
            {
                if (item == null) { list.Add(string.Empty); continue; }
                var str = item as string;
                if (str == null) return false; // not a pure IEnumerable<string>
                list.Add(str);
            }

            result = list;
            return true;
        }


    }
}







