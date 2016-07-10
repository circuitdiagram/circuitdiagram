// XmlWriter.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace CircuitDiagram.IO.Xml
{
    /// <summary>
    /// Writes circuit documents to streams in the XML format.
    /// </summary>
    public class XmlWriter : IElementDocumentWriter
    {
        public string PluginPartName { get { return "Xml Writer"; } }

        public string FileTypeName { get { return "XML Files"; } }

        public string FileTypeExtension { get { return ".xml"; } }

        /// <summary>
        /// Options for saving.
        /// </summary>
        public ISaveOptions Options { get; set; }

        /// <summary>
        /// The document to write.
        /// </summary>
        public IODocument Document { get; set; }

        /// <summary>
        /// Initializes the document writer, before the write method is called.
        /// </summary>
        public void Begin()
        {
            // Nothing to initialize
        }

        /// <summary>
        /// Closes the document writer, after the write method has been called.
        /// </summary>
        public void End()
        {
            // Do nothing.
        }

        /// <summary>
        /// Writes the document in XML format to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public void Write(Stream stream)
        {
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("circuit");
            writer.WriteAttributeString("version", "1.1");
            writer.WriteAttributeString("application", ApplicationInfo.Name);
            writer.WriteAttributeString("appversion", ApplicationInfo.Version);
            writer.WriteAttributeString("width", Document.Size.Width.ToString());
            writer.WriteAttributeString("height", Document.Size.Height.ToString());

            // Write components
            foreach (IOComponent component in Document.Components)
            {
                writer.WriteStartElement("component");
                writer.WriteAttributeString("type", component.Type.Name);
                if (component.Location.HasValue)
                {
                    writer.WriteAttributeString("x", component.Location.Value.X.ToString());
                    writer.WriteAttributeString("y", component.Location.Value.Y.ToString());
                }
                if (component.Orientation.HasValue)
                    writer.WriteAttributeString("orientation", (component.Orientation == Orientation.Horizontal ? "horizontal" : "vertical"));
                if (component.Size.HasValue)
                    writer.WriteAttributeString("size", component.Size.Value.ToString());
                if (component.IsFlipped.HasValue)
                    writer.WriteAttributeString("flipped", component.IsFlipped.Value.ToString());
                if (component.Type.GUID != Guid.Empty)
                    writer.WriteAttributeString("guid", component.Type.GUID.ToString());

                foreach (IOComponentProperty property in component.Properties)
                    writer.WriteAttributeString(property.Key, property.Value.ToString());

                writer.WriteEndElement();
            }

            // Write wires
            foreach (IOWire wire in Document.Wires)
            {
                writer.WriteStartElement("component");
                writer.WriteAttributeString("type", "wire");
                writer.WriteAttributeString("x", wire.Location.X.ToString());
                writer.WriteAttributeString("y", wire.Location.Y.ToString());
                writer.WriteAttributeString("size", wire.Size.ToString());
                writer.WriteAttributeString("orientation", wire.Orientation == Orientation.Horizontal ? "horizontal" : "vertical");
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
        }
    }
}
