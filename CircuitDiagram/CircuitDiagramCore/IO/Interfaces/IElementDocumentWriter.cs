using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Provides methods to write an IODocument to a stream, in the form of components and/or connections.
    /// </summary>
    public interface IElementDocumentWriter
    {
        /// <summary>
        /// The save options to use.
        /// </summary>
        ISaveOptions Options { get; set; }

        /// <summary>
        /// The document to write.
        /// </summary>
        IODocument Document { get; set; }

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
