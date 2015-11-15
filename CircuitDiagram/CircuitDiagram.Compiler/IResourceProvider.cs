using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Compiler
{
    public interface IResourceProvider
    {
        bool HasResource(string name);
        IResource GetResource(string name);
    }
}
