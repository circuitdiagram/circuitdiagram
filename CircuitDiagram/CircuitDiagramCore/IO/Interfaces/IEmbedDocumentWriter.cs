using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Provides methods to write an IODocument to a stream, with the capability of embedding components.
    /// </summary>
    public interface IEmbedDocumentWriter : IElementDocumentWriter
    {
        /// <summary>
        /// Gets or sets a dictionary containing the component data to embed.
        /// </summary>
        IDictionary<IOComponentType, EmbedComponentData> EmbedComponents { get; set; }
    }
}
