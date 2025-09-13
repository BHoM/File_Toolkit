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

namespace BH.Engine.Adapters.File
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Read a CSV file into a 2D object array, parsing each column using CsvConfig.ColumnDataFormats when provided.")]
        [Input("filePath", "Path to the CSV file.")]
        [Input("settings", "CSV settings including delimiter, decimal separator, and per-column formats. If null, defaults are used.")]
        [Input("active", "Boolean used to trigger the function.")]
        public static object[,] ReadFromCsvFile(string filePath, CsvConfig settings = null, bool active = false)
        {
            if (!active)
                return new object[0, 0];

            if (string.IsNullOrWhiteSpace(filePath))
            {
                BH.Engine.Base.Compute.RecordError("The file path must not be empty.");
                return new object[0, 0];
            }

            if (!System.IO.File.Exists(filePath))
            {
                BH.Engine.Base.Compute.RecordError($"The file `{filePath}` does not exist.");
                return new object[0, 0];
            }

            if (settings == null)
                settings = new CsvConfig();

            var delim = settings.Delimiter ?? "\t";
            string[] lines;
            try
            {
                // Note: ReadAllLines splits on Environment.NewLine (handles \r\n and \n)
                lines = System.IO.File.ReadAllLines(filePath);
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError($"Error reading file:\n\t{e}");
                return new object[0, 0];
            }

            if (lines.Length == 0)
                return new object[0, 0];

            // 1) Parse lines into raw string rows (CSV rules: quotes + escaped quotes)
            var rawRows = new List<string[]>();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                // Skip completely empty lines
                if (string.IsNullOrEmpty(line))
                    continue;

                rawRows.Add(SplitCsvLine(line, delim));
            }

            if (rawRows.Count == 0)
                return new object[0, 0];

            // 2) Normalize columns (pad ragged rows to max width)
            int rAll = rawRows.Count;
            int c = 0;
            for (int i = 0; i < rAll; i++)
                if (rawRows[i].Length > c) c = rawRows[i].Length;

            if (c == 0)
                return new object[0, 0];

            // 3) Decide data start index based on IncludeHeader
            int startRow = (settings.IncludeHeader && rAll > 0) ? 1 : 0;
            int rData = Math.Max(0, rAll - startRow);

            var table = new object[rData, c];

            // 4) Parse cells with column-aware formats
            for (int i = 0; i < rData; i++)
            {
                var src = rawRows[i + startRow];
                for (int j = 0; j < c; j++)
                {
                    string cell = j < src.Length ? src[j] : null;
                    table[i, j] = ParseCell(cell, j, settings);
                }
            }

            return table;
        }

        /***************************************************/
        /**** Private Helpers                           ****/
        /***************************************************/

        // CSV splitter: handles quotes, escaped quotes (""), and multi-char delimiters (e.g., "\t")
        private static string[] SplitCsvLine(string line, string delimiter)
        {
            if (string.IsNullOrEmpty(line))
                return new string[0];

            var cells = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;
            int i = 0;
            int n = line.Length;
            int dlen = string.IsNullOrEmpty(delimiter) ? 0 : delimiter.Length;

            while (i < n)
            {
                char ch = line[i];

                if (ch == '"')
                {
                    if (inQuotes && i + 1 < n && line[i + 1] == '"')
                    {
                        // Escaped quote -> add one quote
                        current.Append('"');
                        i += 2;
                        continue;
                    }
                    inQuotes = !inQuotes;
                    i++;
                    continue;
                }

                if (!inQuotes && dlen > 0 && i + dlen <= n &&
                    string.CompareOrdinal(line, i, delimiter, 0, dlen) == 0)
                {
                    // End of cell
                    cells.Add(current.ToString());
                    current.Length = 0;
                    i += dlen;
                    continue;
                }

                current.Append(ch);
                i++;
            }

            cells.Add(current.ToString());
            return cells.ToArray();
        }

        // Column-aware parsing with fallbacks:
        // - If ColumnDataFormats[j] exists, parse by that contract
        // - Else, try bool/(1/0), numeric (with DecimalSeparator), date (several formats), else string
        private static object ParseCell(string raw, int columnIndex, CsvConfig settings)
        {
            if (string.IsNullOrEmpty(raw))
                return null;

            bool hasColumnFormat =
                settings.ColumnDataFormats != null &&
                columnIndex >= 0 &&
                columnIndex < settings.ColumnDataFormats.Count;

            if (hasColumnFormat)
            {
                switch (settings.ColumnDataFormats[columnIndex])
                {
                    case StringType.Boolean:
                        return ParseBool(raw, settings);

                    case StringType.Numeric:
                        {
                            double num;
                            if (TryParseNumber(raw, settings.DecimalSeparator, out num))
                                return num;
                            return null;
                        }

                    case StringType.Date:
                        {
                            DateTime dt;
                            if (TryParseDate(raw, settings.DateTimeFormat, out dt))
                                return dt;
                            // If date parsing fails but it's a double OA, try that as a last resort
                            double serial;
                            if (double.TryParse(raw.Replace(settings.DecimalSeparator, "."), NumberStyles.Any, CultureInfo.InvariantCulture, out serial))
                            {
                                try
                                {
                                    return DateTime.FromOADate(serial);
                                }
                                catch { /* ignore */ }
                            }
                            return null;
                        }

                    case StringType.Text:
                        return raw;
                }
            }

            // ---- Heuristic fallback (no explicit column format) ----

            // Bool
            var bParsed = ParseBool(raw, settings);
            if (bParsed != null)
                return bParsed.Value;

            // Number
            double d;
            if (TryParseNumber(raw, settings.DecimalSeparator, out d))
                return d;

            // Date (ISO/US/EU tries + OA double fallback)
            DateTime when;
            if (TryParseDate(raw, settings.DateTimeFormat, out when))
                return when;

            double serial2;
            if (double.TryParse(raw.Replace(settings.DecimalSeparator, "."), NumberStyles.Any, CultureInfo.InvariantCulture, out serial2))
            {
                try
                {
                    return DateTime.FromOADate(serial2);
                }
                catch { /* ignore */ }
            }

            // Text as-is
            return raw;
        }

        private static bool? ParseBool(string raw, CsvConfig settings)
        {
            if (string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(raw, "false", StringComparison.OrdinalIgnoreCase)) return false;

            if (settings.BooleanAsNumber)
            {
                if (raw == "1") return true;
                if (raw == "0") return false;
            }
            return null;
        }

        private static bool TryParseNumber(string raw, string decimalSeparator, out double value)
        {
            value = 0d;
            if (string.IsNullOrEmpty(raw))
                return false;

            // Normalise custom decimal separator to "."
            var norm = string.IsNullOrEmpty(decimalSeparator) || decimalSeparator == "."
                       ? raw
                       : raw.Replace(decimalSeparator, ".");

            return double.TryParse(norm, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        // Date parsing guided by DateFormatOptions (date-only by default), with tolerant fallbacks.
        private static bool TryParseDate(string raw, DateFormatOptions option, out DateTime dt)
        {
            dt = default(DateTime);
            if (string.IsNullOrEmpty(raw))
                return false;

            string[] patterns;
            switch (option)
            {
                case DateFormatOptions.ISO8601:
                    patterns = new[]
                    {
                        "o",
                        "yyyy-MM-ddTHH:mm:ss",
                        "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
                        "yyyy-MM-dd"
                    };
                    break;

                case DateFormatOptions.US:
                    patterns = new[]
                    {
                        "MM/dd/yyyy",
                        "M/d/yyyy",
                        "MM/dd/yy"
                    };
                    break;

                case DateFormatOptions.EU:
                default:
                    patterns = new[]
                    {
                        "dd/MM/yyyy",
                        "d/M/yyyy",
                        "dd/MM/yy"
                    };
                    break;
            }

            for (int i = 0; i < patterns.Length; i++)
            {
                DateTime tmp;
                if (DateTime.TryParseExact(raw, patterns[i], CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out tmp))
                {
                    dt = tmp;
                    return true;
                }
            }

            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return true;

            return false;
        }
    }
}