using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CircuitDiagram.Render;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Provides methods to write an IODocument to a stream, in the form of an image.
    /// </summary>
    public interface IVisualDocumentWriter
    {
        /// <summary>
        /// The save options to use.
        /// </summary>
        ISaveOptions Options { get; set; }

        /// <summary>
        /// The <see cref="IRenderContext"/> to render the document to.
        /// </summary>
        IRenderContext RenderContext { get; }

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
