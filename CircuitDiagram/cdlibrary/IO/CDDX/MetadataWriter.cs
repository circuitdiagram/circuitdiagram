using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Packaging;
using System.IO;
using System.Xml;

namespace CircuitDiagram.IO.CDDX
{
    public static class MetadataWriter
    {
        private const string DublinCore = "http://purl.org/dc/elements/1.1/";

        public static void WriteMetadata(CircuitDocument document, Package package)
        {
            // Write core properties
            Uri coreUri = PackUriHelper.CreatePartUri(new Uri("docProps\\core.xml", UriKind.Relative));
            PackagePart corePart = package.CreatePart(coreUri, System.Net.Mime.MediaTypeNames.Text.Xml, CompressionOption.Normal);
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
            PackagePart extendedPart = package.CreatePart(extendedUri, System.Net.Mime.MediaTypeNames.Text.Xml, CompressionOption.Normal);
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
