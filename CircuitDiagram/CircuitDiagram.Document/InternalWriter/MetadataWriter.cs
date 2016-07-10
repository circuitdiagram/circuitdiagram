// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CircuitDiagram.Circuit;

namespace CircuitDiagram.Document.InternalWriter
{
    class MetadataWriter
    {
        public void Write(CircuitDocument document, Stream stream)
        {
            var xml = GenerateCorePropertiesXml(document);

            var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "\t"
            });

            xml.WriteTo(writer);
            writer.Flush();
        }

        private XDocument GenerateCorePropertiesXml(CircuitDocument document)
        {
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var root = new XElement(Namespaces.CoreProperties + "coreProperties");
            xml.Add(root);

            if (!string.IsNullOrEmpty(document.Metadata.Creator))
                root.Add(new XElement(Namespaces.DublinCore + "creator", document.Metadata.Creator));

            if (!string.IsNullOrEmpty(document.Metadata.Title))
                root.Add(new XElement(Namespaces.DublinCore + "title", document.Metadata.Creator));

            if (!string.IsNullOrEmpty(document.Metadata.Description))
                root.Add(new XElement(Namespaces.DublinCore + "decsription", document.Metadata.Creator));

            if (document.Metadata.Created.HasValue)
                root.Add(new XElement(Namespaces.DublinCore + "date",
                    document.Metadata.Created.Value.ToUniversalTime().ToString("u", CultureInfo.InvariantCulture)));

            return xml;
        }
    }
}
