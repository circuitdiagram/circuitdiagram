using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Drawing
{
    public interface IPngDrawingContext : IDrawingContext
    {
        void WriteAsPng(Stream stream);
    }
}
