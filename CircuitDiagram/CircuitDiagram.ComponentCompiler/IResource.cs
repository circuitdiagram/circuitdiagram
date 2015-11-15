using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Compiler
{
    public interface IResource
    {
        string MimeType { get; }
        Stream Open();
    }
}
