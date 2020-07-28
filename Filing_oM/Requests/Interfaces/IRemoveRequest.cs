using BH.oM.Base;
using BH.oM.Humans;
using System.IO.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using BH.oM.Data.Requests;

namespace BH.oM.Filing
{
    public class RemoveRequest : IRequest
    {
        [Description("Full Paths of Directory and/or Files to be Removed.")]
        public List<string> ToRemove { get; set; } = new List<string>();
    }
}
