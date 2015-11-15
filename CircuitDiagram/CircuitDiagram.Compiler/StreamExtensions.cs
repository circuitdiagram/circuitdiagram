using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Compiler
{
    static class StreamExtensions
    {
        public static string StreamToString(this Stream stream)
        {
            var fs = stream as FileStream;
            if (fs != null)
                return fs.Name;

            return stream.ToString();
        }
    }
}
