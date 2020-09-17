﻿using System;
using System.Security.AccessControl;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BH.oM.Adapters.Filing;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Adapter.Filing
{
    public partial class FilingAdapter
    {
        /*******************************************/
        /**** Methods                           ****/
        /*******************************************/

        [Input("parentDirectory","Path of parent Directory of the file. You can also specify a string path.")]
        [Input("fullFileName", "Name of the file, INCLUDING Extension.")]
        [Input("content", "The content of the file.")]
        [Description("Creates a FileSystem-hosted object.")]
        public static FSFile CreateFSFile(FSDirectory parentDirectory, string fullFileName, List<object> content = null)
        {
            if (!Path.HasExtension(fullFileName))
            {
                BH.Engine.Reflection.Compute.RecordError($"Please include the extension in the {nameof(fullFileName)}.");
                return null;
            }

            return new oM.Adapters.Filing.FSFile()
            {
                ParentDirectory = parentDirectory,
                Name = fullFileName,
                Content = content
            };
        }

        /*******************************************/
    }
}