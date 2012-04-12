// MetadataReader.cs
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
    static class MetadataReader
    {
        private const string DublinCore = "http://purl.org/dc/elements/1.1/";

        public static void ReadMetadata(Package package, CircuitDocument document)
        {
            // Read core properties
            PackageRelationship coreRelationship = package.GetRelationshipsByType(RelationshipTypes.CoreProperties).FirstOrDefault();
            if (coreRelationship != null)
            {
                PackagePart corePart = package.GetPart(coreRelationship.TargetUri);
                try
                {
                    using (Stream coreStream = corePart.GetStream(FileMode.Open))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(coreStream);

                        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                        namespaceManager.AddNamespace("cp", "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/metadata/core-properties");
                        namespaceManager.AddNamespace("dc", DublinCore);

                        XmlNode contributor = doc.SelectSingleNode("cp:coreProperties/dc:contributor", namespaceManager);
                        if (contributor != null)
                            document.Metadata.DublinCore.Contributor = contributor.InnerText;
                        XmlNode creator = doc.SelectSingleNode("cp:coreProperties/dc:creator", namespaceManager);
                        if (creator != null)
                            document.Metadata.DublinCore.Creator = creator.InnerText;
                        XmlNode date = doc.SelectSingleNode("cp:coreProperties/dc:date", namespaceManager);
                        if (date != null)
                        {
                            DateTime dateTime;
                            if (DateTime.TryParse(date.InnerText, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out dateTime))
                                document.Metadata.DublinCore.Date = dateTime;
                        }
                        XmlNode description = doc.SelectSingleNode("cp:coreProperties/dc:description", namespaceManager);
                        if (description != null)
                            document.Metadata.DublinCore.Description = description.InnerText;
                        XmlNode title = doc.SelectSingleNode("cp:coreProperties/dc:title", namespaceManager);
                        if (title != null)
                            document.Metadata.DublinCore.Title = title.InnerText;
                    }
                }
                catch { }
            }

            // Read extended properties
            PackageRelationship extendedRelationship = package.GetRelationshipsByType(RelationshipTypes.ExtendedProperties).FirstOrDefault();
            if (extendedRelationship != null)
            {
                PackagePart extendedPart = package.GetPart(extendedRelationship.TargetUri);
                try
                {
                    using (Stream extendedStream = extendedPart.GetStream(FileMode.Open))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(extendedStream);

                        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                        namespaceManager.AddNamespace("cp", "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/metadata/extended-properties");

                        XmlNode application = doc.SelectSingleNode("cp:extendedProperties/cp:application", namespaceManager);
                        if (application != null)
                            document.Metadata.Extended.Application = application.InnerText;
                        XmlNode appVersion = doc.SelectSingleNode("cp:extendedProperties/cp:appVersion", namespaceManager);
                        if (appVersion != null)
                            document.Metadata.Extended.AppVersion = appVersion.InnerText;
                    }
                }
                catch { }
            }
        }
    }
}
