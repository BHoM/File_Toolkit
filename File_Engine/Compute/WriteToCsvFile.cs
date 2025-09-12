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

using BH.oM.Adapters.File;
using BH.oM.Base.Attributes;
using System;
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

        [Description("Write a CSV file with the input data or objects.")]
        [Input("data", "Data to write to the file.")]
        [Input("filePath", "Path to the file.")]
        [Input("settings", "Settings to use when writing the CSV file. If null, default settings will be used.")]
        [Input("replace", "If the file exists, you need to set this to true in order to allow overwriting it.")]
        [Input("active", "Boolean used to trigger the function.")]
        public static bool WriteToCsvFile(object data, string filePath, CsvSettings settings = null, bool replace = false, bool active = false)
        {
            if (!active || data == null)
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
            string table = FromObject(data,settings);
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

        // Convert a list of objects to a CSV text string, taking care of the following types: List<List<object>>, List<object>, object[,], object[][], object[]
        public static string FromObject(this object obj, CsvSettings settings = null)
        {
            if (settings == null)
                settings = new CsvSettings();

            var delim = settings.Delimiter ?? "\t";

            // 1) Normalize to list of rows (each row = string[])
            var rows = new List<string[]>();

            switch (obj)
            {
                // --- PRIMARY: Rectangular arrays of objects ---
                case object[,] rect:
                    {
                        int r = rect.GetLength(0);
                        int c = rect.GetLength(1);
                        for (int i = 0; i < r; i++)
                        {
                            var row = new string[c];
                            for (int j = 0; j < c; j++)
                                row[j] = ToCellString(rect[i, j], settings);
                            rows.Add(row);
                        }
                        break;
                    }

                // --- PRIMARY: Jagged arrays of objects ---
                case object[][] jagged:
                    {
                        foreach (var inner in jagged ?? new object[0][])
                        {
                            var src = inner ?? new object[0];
                            var row = new string[src.Length];
                            for (int j = 0; j < src.Length; j++)
                                row[j] = ToCellString(src[j], settings);
                            rows.Add(row);
                        }
                        break;
                    }

                // --- SECONDARY: Sequences of sequences of objects ---
                case IEnumerable<IEnumerable<object>> seqOfSeq:
                    {
                        foreach (var inner in seqOfSeq)
                        {
                            var items = inner ?? new object[0];
                            var rowList = new List<string>();
                            foreach (var it in items)
                                rowList.Add(ToCellString(it, settings));
                            rows.Add(rowList.ToArray());
                        }
                        break;
                    }

                // --- SECONDARY: Sequence of objects (single row); exclude string itself ---
                case IEnumerable<object> seq when !(obj is string):
                    {
                        var rowList = new List<string>();
                        foreach (var it in seq)
                            rowList.Add(ToCellString(it, settings));
                        rows.Add(rowList.ToArray());
                        break;
                    }

                // Single string -> one cell
                case string s:
                    {
                        rows.Add(new[] { s });
                        break;
                    }

                // Fallback: single cell from ToString()
                default:
                    {
                        rows.Add(new[] { ToCellString(obj, settings) });
                        break;
                    }
            }

            if (rows.Count == 0)
                return string.Empty;

            // 2) Pad ragged rows to the maximum width
            int maxCols = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i] ?? new string[0];
                if (r.Length > maxCols) maxCols = r.Length;
            }
            if (maxCols == 0) maxCols = 1;

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i] ?? new string[0];
                if (r.Length < maxCols)
                {
                    var padded = new string[maxCols];
                    Array.Copy(r, padded, r.Length); // remaining are null -> treated as empty
                    rows[i] = padded;
                }
            }

            // 3) Optional 1-based index column
            if (settings.IncludeIndex)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var current = rows[i];
                    var newRow = new string[current.Length + 1];
                    newRow[0] = (i + 1).ToString(CultureInfo.InvariantCulture);
                    Array.Copy(current, 0, newRow, 1, current.Length);
                    rows[i] = newRow;
                }
            }

            // 4) Serialize with CSV escaping
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

        /*******************************************/
        /**** Private Methods                  *****/
        /*******************************************/

        private static string ToCellString(object value, CsvSettings settings)
        {
            if (value == null)
                return string.Empty;

            // string
            var s = value as string;
            if (s != null)
                return s;

            // numeric (round if Digit set)
            if (IsNumeric(value))
            {
                double d = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);

                string raw;
                if (settings.Digit.HasValue)
                {
                    int digits = (int) settings.Digit.Value;
                    var format = digits > 0 ? "0." + new string('#', digits) : "0";
                    raw = Math.Round(d, digits).ToString(format, CultureInfo.InvariantCulture);
                }
                else
                {
                    raw = d.ToString("G", CultureInfo.InvariantCulture);
                }

                // Apply custom decimal separator if needed
                if (settings.DecimalSeparator != "." && raw.Contains("."))
                {
                    raw = raw.Replace(".", settings.DecimalSeparator);
                }

                return raw;
            }

            // Date/Time
            if (value is DateTime dt)
                return dt.FormatDate(settings.DateTimeFormat);


            // Bool
            if (value is bool b)
                return b ? "true" : "false";

            // Enum
            var type = value.GetType();
            if (type.IsEnum)
                return System.Convert.ToString(value, CultureInfo.InvariantCulture);

            // IFormattable (decimal, Guid, etc.)
            var formattable = value as IFormattable;

            if (formattable != null)
                return formattable.ToString(null, CultureInfo.InvariantCulture);

            if (settings.IncludeObjects)
            {
                // Fallback: use ToString() if overridden
                var toString = value.ToString();
                if (toString != null && toString != type.FullName)
                    return toString;
                // Fallback: type name
                return $"<{type.Name}>";
            }

            // Fallback
            return string.Empty;
        }

        private static bool IsNumeric(object value)
        {
            return value is byte || value is sbyte ||
                   value is short || value is ushort ||
                   value is int || value is uint ||
                   value is long || value is ulong ||
                   value is float || value is double ||
                   value is decimal;
        }

        private static string EscapeCsv(string input, string delimiter)
        {
            if (input == null) return string.Empty;

            bool mustQuote =
                input.IndexOf('"') >= 0 ||
                input.IndexOf('\n') >= 0 ||
                input.IndexOf('\r') >= 0 ||
                (!string.IsNullOrEmpty(delimiter) && input.IndexOf(delimiter, StringComparison.Ordinal) >= 0);

            if (!mustQuote)
                return input;

            var doubled = input.Replace("\"", "\"\"");
            return "\"" + doubled + "\"";
        }

        private static string FormatDate(this IFormattable date, DateFormatOptions option)
        {
            switch (option)
            {
                case DateFormatOptions.ISO8601:
                    // round-trip, always UTC if DateTime.Kind is UTC
                    return date.ToString("o", CultureInfo.InvariantCulture);

                case DateFormatOptions.US:
                    // month/day/year
                    return date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

                case DateFormatOptions.EU:
                default:
                    // day/month/year
                    return date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
        }
    }
}
