// BinaryResource.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2015  Sam Fisher
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using CircuitDiagram.IO.Descriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Represents a resource contained in a binary component file.
    /// </summary>
    public class BinaryResource : BinaryDescriptionContentItem
    {
        /// <summary>
        /// Gets the binary content item type ID for this item.
        /// </summary>
        public override BinaryConstants.ContentItemType ItemType
        {
            get { return BinaryConstants.ContentItemType.Resource; }
        }

        /// <summary>
        /// The type of resource.
        /// </summary>
        public BinaryResourceType ResourceType { get; set; }

        /// <summary>
        /// Resource data.
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// Reads the content item from the specified reader.
        /// </summary>
        /// <param name="reader">Binary reader to read from.</param>
        internal override void Read(System.IO.BinaryReader reader, BinaryReadInfo readInfo)
        {
            uint length = reader.ReadUInt32();

            ID = reader.ReadUInt32();
            ResourceType = (BinaryResourceType)reader.ReadUInt32();
            Buffer = reader.ReadBytes((int)length - 8);
        }

        /// <summary>
        /// Writes the content item to the specified writer.
        /// </summary>
        /// <param name="writer">Binary writer to write to.</param>
        internal override void Write(System.IO.BinaryWriter writer)
        {
            writer.Write((uint)(Buffer.Length + 8));

            writer.Write(ID);
            writer.Write((uint)ResourceType);
            writer.Write(Buffer);
        }
    }
}