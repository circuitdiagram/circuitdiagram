using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO.Descriptions
{
    /// <summary>
    /// Represents a single component indside a binary component description file.
    /// </summary>
    public abstract class BinaryDescriptionContentItem
    {
        /// <summary>
        /// The unique ID of the content item.
        /// </summary>
        public uint ID { get; set; }

        /// <summary>
        /// Gets the binary content item type ID for this item.
        /// </summary>
        public abstract BinaryConstants.ContentItemType ItemType { get; }

        /// <summary>
        /// Reads the content item from the specified reader.
        /// </summary>
        /// <param name="reader">Binary reader to read from.</param>
        internal abstract void Read(System.IO.BinaryReader reader, BinaryReadInfo readInfo);

        /// <summary>
        /// Writes the content item to the specified writer.
        /// </summary>
        /// <param name="writer">Binary writer to write to.</param>
        internal abstract void Write(System.IO.BinaryWriter writer);
    }
}
