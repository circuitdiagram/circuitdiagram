// MetadataWriter.cs
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
using System.IO.Packaging;
using System.IO;
using System.Xml;

namespace CircuitDiagram.IO.CDDX
{
    static class MetadataWriter
    {
        private const string DublinCore = "http://purl.org/dc/elements/1.1/";

        public static void WriteMetadata(CircuitDocument document, Package package)
        {
            // Write core properties
            Uri coreUri = PackUriHelper.CreatePartUri(new Uri("docProps\\core.xml", UriKind.Relative));
            PackagePart corePart = package.CreatePart(coreUri, ContentTypeNames.CoreProperties, CompressionOption.Normal);
            package.CreateRelationship(coreUri, TargetMode.Internal, RelationshipTypes.CoreProperties);

            using (var stream = corePart.GetStream(FileMode.Create))
            {
                XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("coreProperties", "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/metadata/core-properties");
                writer.WriteAttributeString("xmlns", "dc", null, DublinCore);

                if (!String.IsNullOrEmpty(document.Metadata.DublinCore.Contributor))
                    writer.WriteElementString("contributor", DublinCore, document.Metadata.DublinCore.Contributor);
                if (!String.IsNullOrEmpty(document.Metadata.DublinCore.Creator))
                    writer.WriteElementString("creator", DublinCore, document.Metadata.DublinCore.Creator);
                if (document.Metadata.DublinCore.Date.HasValue)
                    writer.WriteElementString("date", DublinCore, document.Metadata.DublinCore.Date.Value.ToUniversalTime().ToString("u", System.Globalization.CultureInfo.InvariantCulture));
                if (!String.IsNullOrEmpty(document.Metadata.DublinCore.Description))
                    writer.WriteElementString("description", DublinCore, document.Metadata.DublinCore.Description);
                if (!String.IsNullOrEmpty(document.Metadata.DublinCore.Title))
                    writer.WriteElementString("title", DublinCore, document.Metadata.DublinCore.Title);

                writer.WriteEndElement();
                writer.WriteEndDocument();

                writer.Flush();
            }

            // Write extended properties
            Uri extendedUri = PackUriHelper.CreatePartUri(new Uri("docProps\\extended.xml", UriKind.Relative));
            PackagePart extendedPart = package.CreatePart(extendedUri, ContentTypeNames.ExtendedProperties, CompressionOption.Normal);
            package.CreateRelationship(extendedUri, TargetMode.Internal, RelationshipTypes.ExtendedProperties);

            using (var stream = extendedPart.GetStream(FileMode.Create))
            {
                XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("extendedProperties", "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/metadata/extended-properties");

                writer.WriteElementString("application", ApplicationInfo.Name);
                writer.WriteElementString("appVersion", ApplicationInfo.Version);

                writer.WriteEndElement();
                writer.WriteEndDocument();

                writer.Flush();
            }
        }
    }
}
