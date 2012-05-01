using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Provides methods to read an IODocument from a stream.
    /// </summary>
    public interface IDocumentReader : IDisposable
    {
        /// <summary>
        /// Gets the document loaded from the stream.
        /// </summary>
        IODocument Document { get; }

        /// <summary>
        /// Gets details of the load result.
        /// </summary>
        DocumentLoadResult LoadResult { get; }

        /// <summary>
        /// Attempts to load a document from the stream.
        /// </summary>
        /// <param name="stream">Stream to load from.</param>
        /// <returns>True if succeeded, false otherwise.</returns>
        bool Load(Stream stream);

        /// <summary>
        /// Determines whether the specified component type is embedded within the document.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if it is available, false otherwise.</returns>
        bool IsDescriptionEmbedded(IOComponentType type);

        /// <summary>
        /// Retrieves the specified component type from the document.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>The data if the type is available, null otherwise.</returns>
        EmbedComponentData GetEmbeddedDescription(IOComponentType type);
    }
}
