// CircuitDocumentWriter.cs
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
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Xml;
using CircuitDiagram.Components;

namespace CircuitDiagram.IO
{
    public static class CircuitDocumentWriter
    {
        #region XML
        public static void WriteXml(CircuitDocument document, Stream stream)
        {
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;

            WriteXmlVersion1(writer, document);

            writer.Flush();
        }

        private static void WriteXmlVersion1(XmlTextWriter writer, CircuitDocument document)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("circuit");
            writer.WriteAttributeString("version", "1.1");
            writer.WriteAttributeString("width", document.Size.Width.ToString());
            writer.WriteAttributeString("height", document.Size.Height.ToString());

            foreach (Component component in document.Components)
            {
                writer.WriteStartElement("component");
                writer.WriteAttributeString("type", component.Description.ComponentName);
                writer.WriteAttributeString("x", component.Offset.X.ToString());
                writer.WriteAttributeString("y", component.Offset.Y.ToString());
                writer.WriteAttributeString("orientation", (component.Horizontal ? "horizontal" : "vertical"));
                if (component.Description.CanResize)
                    writer.WriteAttributeString("size", component.Size.ToString());
                if (component.Description.Metadata.GUID != Guid.Empty)
                    writer.WriteAttributeString("guid", component.Description.Metadata.GUID.ToString());

                Dictionary<string, object> properties = new Dictionary<string,object>();
                component.Serialize(properties);
                foreach (KeyValuePair<string, object> property in properties)
                {
                    if (!property.Key.StartsWith("@"))
                        writer.WriteAttributeString(property.Key, property.Value.ToString());
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
        #endregion
    }
}
