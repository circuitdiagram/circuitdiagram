using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// The base interface for all document writers.
    /// </summary>
    public interface IDocumentWriter : IPluginPart
    {
        /// <summary>
        /// Gets the name of the file type associated with this writer.
        /// </summary>
        string FileTypeName { get; }

        /// <summary>
        /// Gets the file extension associated with this writer, including the preceeding period.
        /// </summary>
        string FileTypeExtension { get; }

        /// <summary>
        /// The save options to use.
        /// </summary>
        ISaveOptions Options { get; set; }

        /// <summary>
        /// Initializes the document writer, before the write method is called.
        /// </summary>
        void Begin();

        /// <summary>
        /// Writes the document to the stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        void Write(Stream stream);

        /// <summary>
        /// Closes the document writer, after the write method has been called.
        /// </summary>
        void End();
    }
}
