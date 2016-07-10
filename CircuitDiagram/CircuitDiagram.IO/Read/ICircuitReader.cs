using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.IO.Data;

namespace CircuitDiagram.IO.Read
{
    /// <summary>
    /// Reads a <see cref="CircuitDocument"/> from a stream.
    /// </summary>
    public interface ICircuitReader
    {
        /// <summary>
        /// Gets the name of the file type associated with this reader.
        /// </summary>
        string FileTypeName { get; }

        /// <summary>
        /// Gets the file extension associated with this reader, including the preceeding period.
        /// </summary>
        string FileTypeExtension { get; }

        CircuitDocument ReadCircuit(Stream stream);
    }
}
