using BH.oM.Base;
using BH.oM.Humans;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.Filing
{
    [Description("Identifies a general data Resource, whether a File or a Directory or other, that resides on a File System.")]
    public interface IFSResource : IResource
    {
    }
}
