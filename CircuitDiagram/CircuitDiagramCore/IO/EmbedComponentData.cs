using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Holds the data required to embed a component within a file.
    /// </summary>
    public class EmbedComponentData
    {
        /// <summary>
        /// The stream contatining the data to embed.
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// The content type of the stream.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The file extension for the type of data in the stream.
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Whether the data has already been embedded.
        /// </summary>
        public bool IsEmbedded { get; set; }

        /// <summary>
        /// A custom tag for this item.
        /// </summary>
        public object Tag { get; set; }
    }
}
