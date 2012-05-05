// CDDXWriter.cs
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
using System.IO.Packaging;
using System.Xml;

namespace CircuitDiagram.IO.CDDX
{
    /// <summary>
    /// Writes circuit documents to streams in the CDDX format.
    /// </summary>
    [SaveOptionsType(typeof(CDDXSaveOptions))]
    public class CDDXWriter : IEmbedDocumentWriter, IVisualDocumentWriter
    {
        public string PluginPartName { get { return "CDDX Writer"; } }

        public string FileTypeName { get { return "Circuit Diagram Document"; } }

        public string FileTypeExtension { get { return ".cddx"; } }

        /// <summary>
        /// The CDDX format version this class writes.
        /// </summary>
        public const double FormatVersion = 1.0;

        /// <summary>
        /// Options for saving.
        /// </summary>
        public ISaveOptions Options { get; set; }

        /// <summary>
        /// The document to write.
        /// </summary>
        public IODocument Document { get; set; }

        /// <summary>
        /// Render context for generating a document preview.
        /// </summary>
        public Render.IRenderContext RenderContext { get; private set; }

        /// <summary>
        /// Gets or sets a dictionary containing the component data to embed.
        /// </summary>
        public IDictionary<IOComponentType, EmbedComponentData> EmbedComponents { get; set; }

        /// <summary>
        /// Initializes the document writer, before the write method is called.
        /// </summary>
        public void Begin()
        {
            // Setup rendering for thumbnail
            RenderContext = new CircuitDiagram.Render.XmlRenderer(Document.Size);
        }

        /// <summary>
        /// Closes the document writer, after the write method has been called.
        /// </summary>
        public void End()
        {
            // Do nothing.
        }

        /// <summary>
        /// Writes the document in CDDX format to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public void Write(Stream stream)
        {
            using (MemoryStream tempStream = new MemoryStream())
            {
                using (Package package = ZipPackage.Open(tempStream, FileMode.Create))
                {
                    // Write the main document part
                    WriteMainDocument(package);

                    // Write the metadata parts
                    WriteMetadata(package);

                    // Write the thumbnail part
                    if ((Options as CDDXSaveOptions).Thumbnail)
                        WriteThumbnail(package);
                }
                tempStream.WriteTo(stream);
            }
        }

        /// <summary>
        /// Writes the main document part of a CDDX file, and embeds components as required.
        /// </summary>
        /// <param name="package">The package to write to.</param>
        /// <param name="document">The document containing the metadata to write.</param>
        /// <param name="options">Options for saving.</param>
        void WriteMainDocument(Package package)
        {
            // Prevent EmbedComponents from being null
            if (EmbedComponents == null)
                EmbedComponents = new Dictionary<IOComponentType, EmbedComponentData>();

            // Create the main Document part
            Uri DocumentUri = PackUriHelper.CreatePartUri(new Uri("circuitdiagram\\Document.xml", UriKind.Relative));
            PackagePart DocumentPart = package.CreatePart(DocumentUri, ContentTypeNames.MainDocument, CompressionOption.Normal);
            package.CreateRelationship(DocumentPart.Uri, TargetMode.Internal, RelationshipTypes.Document);

            using (var stream = DocumentPart.GetStream(FileMode.Create))
            {
                XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("circuit", Namespaces.Document);
                writer.WriteAttributeString("version", String.Format("{0:0.0}", FormatVersion));
                writer.WriteAttributeString("xmlns", "r", null, Namespaces.Relationships);
                writer.WriteAttributeString("xmlns", "ec", null, Namespaces.DocumentComponentDescriptions);
                if (EmbedComponents.Count > 0)
                    writer.WriteAttributeString("xmlns", "ec", null, Namespaces.DocumentComponentDescriptions);

                // Document size
                writer.WriteStartElement("properties");
                writer.WriteElementString("width", Document.Size.Width.ToString());
                writer.WriteElementString("height", Document.Size.Height.ToString());
                writer.WriteEndElement();

                // Component types
                int idCounter = 0; // generate IDs for each IOComponentType
                Dictionary<IOComponentType, int> typeIDs = new Dictionary<IOComponentType, int>(); // store IDs for use in <elements> section
                writer.WriteStartElement("definitions");
                foreach (KeyValuePair<string, List<IOComponentType>> collection in Document.GetComponentTypes())
                {
                    writer.WriteStartElement("src");
                    writer.WriteAttributeString("col", collection.Key);

                    foreach (IOComponentType item in collection.Value)
                    {
                        writer.WriteStartElement("add");
                        writer.WriteAttributeString("id", idCounter.ToString());
                        writer.WriteAttributeString("item", item.Item);

                        // Write additional attributes for opening with the same component description
                        if (!String.IsNullOrEmpty(item.Name))
                            writer.WriteAttributeString("name", Namespaces.DocumentComponentDescriptions, item.Name);
                        if (item.GUID != Guid.Empty)
                            writer.WriteAttributeString("guid", Namespaces.DocumentComponentDescriptions, item.GUID.ToString());

                        // Write additional attributes for embedding
                        if (EmbedComponents.ContainsKey(item))
                        {
                            // Generate unique file name
                            Uri descriptionPath = PackUriHelper.CreatePartUri(new Uri("circuitdiagram/components/" + item.Name.Replace(' ', '_') + EmbedComponents[item].FileExtension, UriKind.Relative));
                            int addedInt = 0;
                            while (package.PartExists(descriptionPath))
                            {
                                descriptionPath = PackUriHelper.CreatePartUri(new Uri("circuitdiagram/components/" + item.Name.Replace(' ', '_') + addedInt.ToString() + ".cdcom", UriKind.Relative));
                                addedInt++;
                            }

                            // Write part
                            PackagePart descriptionPart = package.CreatePart(PackUriHelper.CreatePartUri(descriptionPath), EmbedComponents[item].ContentType, CompressionOption.Normal);
                            using (var descriptionStream = descriptionPart.GetStream(FileMode.Create))
                            {
                                // Copy stream
                                int num;
                                byte[] buffer = new byte[4096];
                                while ((num = EmbedComponents[item].Stream.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    descriptionStream.Write(buffer, 0, num);
                                }
                            }
                            PackageRelationship relationship = DocumentPart.CreateRelationship(descriptionPart.Uri, TargetMode.Internal, CDDX.RelationshipTypes.IncludedComponent);

                            // Write the relationship ID
                            writer.WriteAttributeString("id", Namespaces.Relationships, relationship.Id);

                            // Store the relationship ID for use later
                            EmbedComponents[item].Tag = relationship.Id;
                        }
                        else if (EmbedComponents.ContainsKey(item) && !EmbedComponents[item].IsEmbedded)
                        {
                            // Already embedded - write relationship ID
                            writer.WriteAttributeString("id", Namespaces.DocumentComponentDescriptions, EmbedComponents[item].Tag as string);
                        }

                        writer.WriteEndElement();

                        // Store type ID
                        typeIDs.Add(item, idCounter);
                        idCounter++;
                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                // Elements
                writer.WriteStartElement("elements");
                if ((Options as CDDXSaveOptions).Layout)
                {
                    foreach (IOWire wire in Document.Wires)
                    {
                        // Write wire element

                        writer.WriteStartElement("w");
                        writer.WriteAttributeString("x", wire.Location.X.ToString());
                        writer.WriteAttributeString("y", wire.Location.Y.ToString());
                        writer.WriteAttributeString("o", wire.Orientation == Orientation.Horizontal ? "h" : "v");
                        writer.WriteAttributeString("sz", wire.Size.ToString());
                        writer.WriteEndElement();
                    }
                }
                foreach (IOComponent component in Document.Components)
                {
                    // Write component element

                    writer.WriteStartElement("c");
                    if (!String.IsNullOrEmpty(component.ID))
                        writer.WriteAttributeString("id", component.ID);
                    writer.WriteAttributeString("tp", "{" + typeIDs[component.Type].ToString() + "}");

                    // Write layout
                    if ((Options as CDDXSaveOptions).Layout)
                    {
                        if (component.Location.HasValue)
                        {
                            writer.WriteAttributeString("x", component.Location.Value.X.ToString());
                            writer.WriteAttributeString("y", component.Location.Value.Y.ToString());
                        }
                        if (component.Orientation.HasValue)
                            writer.WriteAttributeString("o", component.Orientation == Orientation.Horizontal ? "h" : "v");
                        if (component.Size.HasValue)
                            writer.WriteAttributeString("sz", component.Size.Value.ToString());
                        if (component.IsFlipped.HasValue)
                            writer.WriteAttributeString("flp", (component.IsFlipped.Value ? "true" : "false"));
                    }

                    // Write properties
                    if (component.Properties.Count > 0)
                    {
                        writer.WriteStartElement("prs");

                        foreach (IOComponentProperty property in component.Properties)
                        {
                            writer.WriteStartElement("p");
                            writer.WriteAttributeString("k", property.Key.ToString());
                            writer.WriteAttributeString("v", property.Value.ToString());
                            if (!property.IsStandard)
                                writer.WriteAttributeString("st", Namespaces.DocumentComponentDescriptions, "false");
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    // Write connections
                    if ((Options as CDDXSaveOptions).Connections && component.Connections.Count > 0)
                    {
                        writer.WriteStartElement("cns");

                        foreach (KeyValuePair<string, string> connection in component.Connections)
                        {
                            writer.WriteStartElement("cn");
                            writer.WriteAttributeString("pt", connection.Key);
                            writer.WriteAttributeString("id", connection.Value);
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();

                writer.Flush();
            }
        }

        /// <summary>
        /// Writes the metadata parts of a CDDX file.
        /// </summary>
        /// <param name="package">The package to write to.</param>
        /// <param name="document">Options for saving.</param>
        void WriteMetadata(Package package)
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
                writer.WriteAttributeString("xmlns", "dc", null, Namespaces.DublinCore);

                if (!String.IsNullOrEmpty(Document.Metadata.Creator))
                    writer.WriteElementString("creator", Namespaces.DublinCore, Document.Metadata.Creator);
                if (!String.IsNullOrEmpty(Document.Metadata.Description))
                    writer.WriteElementString("description", Namespaces.DublinCore, Document.Metadata.Description);
                if (!String.IsNullOrEmpty(Document.Metadata.Title))
                    writer.WriteElementString("title", Namespaces.DublinCore, Document.Metadata.Title);

                if (Document.Metadata.Created.HasValue)
                    writer.WriteElementString("date", Namespaces.DublinCore, Document.Metadata.Created.Value.ToUniversalTime().ToString("u", System.Globalization.CultureInfo.InvariantCulture));

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

        /// <summary>
        /// Writes the thumbnail part of a CDDX file.
        /// </summary>
        /// <param name="package">The package to write to.</param>
        /// <param name="document">The document contatining the preview to write.</param>
        void WriteThumbnail(Package package)
        {
            // Create the thumbnail part
            Uri thumbnailUri = PackUriHelper.CreatePartUri(new Uri("docProps\\thumbnail.xml", UriKind.Relative));
            PackagePart thumbnailPart = package.CreatePart(thumbnailUri, CircuitDiagram.Render.XmlRenderer.PreviewContentType, CompressionOption.Normal);
            package.CreateRelationship(thumbnailPart.Uri, TargetMode.Internal, RelationshipTypes.Thumbnail);
            using (var stream = thumbnailPart.GetStream(FileMode.Create))
            {
                (RenderContext as CircuitDiagram.Render.XmlRenderer).WriteXmlTo(stream);
            }
        }
    }
}
