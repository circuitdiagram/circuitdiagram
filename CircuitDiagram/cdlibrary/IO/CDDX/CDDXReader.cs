// CDDXReader.cs
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
    /// Reads a circuit document in the CDDX format from a stream.
    /// </summary>
    public class CDDXReader : IDocumentReader
    {
        public string PluginPartName { get { return "CDDX Reader"; } }

        public string FileTypeName { get { return "Circuit Diagram Document"; } }

        public string FileTypeExtension { get { return ".cddx"; } }

        /// <summary>
        /// The CDDX format version that this class is capable of reading.
        /// </summary>
        public const double FormatVersion = 1.0;

        /// <summary>
        /// The package which has been opened from the stream.
        /// </summary>
        private Package m_package;

        /// <summary>
        /// Holds the PackageParts for embedded descriptions, allowing subsequent retrieval.
        /// </summary>
        Dictionary<IOComponentType, PackagePart> m_typeParts;

        /// <summary>
        /// Determines whether the format being read is a newer version than is supported.
        /// </summary>
        bool m_newerVersion;

        /// <summary>
        /// The document loaded from the CDDX stream.
        /// </summary>
        public IODocument Document { get; private set; }

        /// <summary>
        /// Gets details of the load result.
        /// </summary>
        public DocumentLoadResult LoadResult { get; private set; }

        /// <summary>
        /// Initializes a new reader.
        /// </summary>
        public CDDXReader()
        {
            m_typeParts = new Dictionary<IOComponentType, PackagePart>();
        }

        /// <summary>
        /// Attempts to read a CDDX document from the specified stream.
        /// </summary>
        /// <param name="stream">The stream containing a CDDX document to load.</param>
        /// <returns>True if the load was successful, false otherwise.</returns>
        public bool Load(Stream stream)
        {
            try
            {
                // Set up result
                LoadResult = new DocumentLoadResult();
                LoadResult.Format = "Circuit Diagram Document";
                bool success = true;

                m_package = ZipPackage.Open(stream);

                // Load format properties, if available
                ReadFormatProperties();

                // Load main document
                success = LoadMainDocument(m_package);

                // Load metadata
                ReadMetadata();

                return success;
            }
            catch (FileFormatException)
            {
                // Try opening as legacy format
                IODocument circuitDocument;
                DocumentLoadResult loadResult;
                stream.Seek(0, SeekOrigin.Begin);
                bool succeeded = LegacyCDDXReader.Read(stream, out circuitDocument, out loadResult);
                Document = circuitDocument;
                LoadResult = loadResult;
                return succeeded;
            }
            catch
            {
                m_package = null;
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified component type is embedded within the document.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if it is available, false otherwise.</returns>
        public bool IsDescriptionEmbedded(IOComponentType type)
        {
            return m_typeParts.ContainsKey(type);
        }

        /// <summary>
        /// Retrieves the specified component type from the document.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>The data stream if the type is available, null otherwise.</returns>
        public EmbedComponentData GetEmbeddedDescription(IOComponentType type)
        {
            PackagePart typePart;
            if (m_typeParts.TryGetValue(type, out typePart))
            {
                EmbedComponentData data = new EmbedComponentData();
                data.ContentType = typePart.ContentType;
                data.FileExtension = Path.GetExtension(typePart.Uri.ToString());
                data.Stream = typePart.GetStream(FileMode.Open);
                data.IsEmbedded = true;
                return data;
            }
            else
                return null;
        }

        /// <summary>
        /// Closes the package.
        /// </summary>
        public void Dispose()
        {
            if (m_package != null)
                m_package.Close();
            m_package = null;
        }

        /// <summary>
        /// Loads the main document part of a CDDX file.
        /// </summary>
        /// <param name="package">The package containing the document to load.</param>
        /// <returns>True if succeeded, false otherwise.</returns>
        bool LoadMainDocument(Package package)
        {
            // Open the document part
            PackageRelationship documentRelationship = package.GetRelationshipsByType(RelationshipTypes.Document).FirstOrDefault();
            PackagePart documentPart = package.GetPart(documentRelationship.TargetUri);
            using (Stream docStream = documentPart.GetStream(FileMode.Open))
            {
                Document = new IODocument();

                XmlDocument doc = new XmlDocument();
                doc.Load(docStream);

                // Set up namespaces
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("cdd", Namespaces.Document);
                namespaceManager.AddNamespace("ec", Namespaces.DocumentComponentDescriptions);

                // Read version
                double version = 1.0;
                XmlElement rootElement = doc.SelectSingleNode("/cdd:circuit", namespaceManager) as XmlElement;
                if (rootElement != null && rootElement.HasAttribute("version"))
                    double.TryParse(rootElement.Attributes["version"].InnerText, out version);
                if (version > FormatVersion)
                    m_newerVersion = true;

                if (m_newerVersion)
                    LoadResult.Type = DocumentLoadResultType.SuccessNewerVersion;
                else
                    LoadResult.Type = DocumentLoadResultType.Success;

                // Read size
                double width = 640d;
                double height = 480d;
                XmlNode widthNode = doc.SelectSingleNode("/cdd:circuit/cdd:properties/cdd:width", namespaceManager);
                if (widthNode != null)
                    double.TryParse(widthNode.InnerText, out width);
                XmlNode heightNode = doc.SelectSingleNode("/cdd:circuit/cdd:properties/cdd:height", namespaceManager);
                if (heightNode != null)
                    double.TryParse(heightNode.InnerText, out height);
                Document.Size = new System.Windows.Size(width, height);

                // Read sources
                m_typeParts.Clear();
                Dictionary<string, IOComponentType> componentTypes = new Dictionary<string, IOComponentType>(); // for use when loading component elements
                XmlNodeList componentSourceNodes = doc.SelectNodes("/cdd:circuit/cdd:definitions/cdd:src", namespaceManager);
                foreach (XmlElement source in componentSourceNodes)
                {
                    // Read collection
                    string collection = null;
                    if (source.HasAttribute("col"))
                        collection = source.Attributes["col"].InnerText;

                    foreach (XmlElement addType in source.SelectNodes("cdd:add", namespaceManager))
                    {
                        // Read item
                        string typeId = addType.Attributes["id"].InnerText;
                        string item = null;
                        if (addType.HasAttribute("item"))
                            item = addType.Attributes["item"].InnerText;

                        // Read additional attributes for opening with the same component description
                        string name = null;
                        if (addType.HasAttribute("name", Namespaces.DocumentComponentDescriptions))
                            name = addType.Attributes["name", Namespaces.DocumentComponentDescriptions].InnerText;
                        Guid guid = Guid.Empty;
                        if (addType.HasAttribute("guid", Namespaces.DocumentComponentDescriptions))
                            guid = new Guid(addType.Attributes["guid", Namespaces.DocumentComponentDescriptions].InnerText);

                        // Create new IOComponentType
                        IOComponentType type = new IOComponentType(collection, item);
                        type.Name = name;
                        type.GUID = guid;

                        // Read additional attributes for embedding
                        if (addType.HasAttribute("id", Namespaces.Relationships))
                        {
                            string relationshipId = addType.Attributes["id", Namespaces.Relationships].InnerText;
                            PackageRelationship relationship = documentPart.GetRelationship(relationshipId);
                            PackagePart embeddedTypePart = package.GetPart(PackUriHelper.ResolvePartUri(documentPart.Uri, relationship.TargetUri));
                            m_typeParts.Add(type, embeddedTypePart);
                        }

                        componentTypes.Add(typeId, type);
                    }
                }

                // Read wire elements
                XmlNodeList wires = doc.SelectNodes("/cdd:circuit/cdd:elements//cdd:w", namespaceManager);
                foreach (XmlElement wire in wires)
                {
                    // Read wire
                    double x = double.Parse(wire.Attributes["x"].InnerText);
                    double y = double.Parse(wire.Attributes["y"].InnerText);
                    Orientation orientation = Orientation.Vertical;
                    if (wire.HasAttribute("o") && wire.Attributes["o"].InnerText.ToLowerInvariant() == "h")
                        orientation = Orientation.Horizontal;
                    double size = 10d;
                    if (wire.HasAttribute("sz"))
                        size = double.Parse(wire.Attributes["sz"].InnerText);

                    Document.Wires.Add(new IOWire(new System.Windows.Point(x, y), size, orientation));
                }

                // Read component elements
                XmlNodeList componentElements = doc.SelectNodes("/cdd:circuit/cdd:elements//cdd:c", namespaceManager);
                foreach (XmlElement element in componentElements)
                {
                    // Read component element
                    string id = null;
                    if (element.HasAttribute("id"))
                        id = element.Attributes["id"].InnerText;
                    string typeId = element.Attributes["tp"].InnerText;

                    // Read layout information
                    System.Windows.Point? location = null;
                    if (element.HasAttribute("x") && element.HasAttribute("y"))
                        location = new System.Windows.Point(double.Parse(element.Attributes["x"].InnerText), double.Parse(element.Attributes["y"].InnerText));
                    double? size = null;
                    if (element.HasAttribute("sz"))
                        size = double.Parse(element.Attributes["sz"].InnerText);
                    Orientation? orientation = null;
                    if (element.HasAttribute("o") && element.Attributes["o"].InnerText.ToLowerInvariant() == "h")
                        orientation = Orientation.Horizontal;
                    else if (element.HasAttribute("o") && element.Attributes["o"].InnerText.ToLowerInvariant() == "v")
                        orientation = Orientation.Vertical;
                    bool? flipped = null;
                    if (element.HasAttribute("flp") && element.Attributes["flp"].InnerText.ToLowerInvariant() == "false")
                        flipped = false;
                    else if (element.HasAttribute("flp"))
                        flipped = true;

                    // Read properties
                    List<IOComponentProperty> properties = new List<IOComponentProperty>();
                    XmlNodeList propertyNodes = element.SelectNodes("cdd:prs/cdd:p", namespaceManager);
                    foreach (XmlElement property in propertyNodes)
                    {
                        // Read property
                        string key = property.Attributes["k"].InnerText;
                        string value = property.Attributes["v"].InnerText;
                        bool isStandard = true;
                        if (property.HasAttribute("st", Namespaces.DocumentComponentDescriptions) && property.Attributes["st", Namespaces.DocumentComponentDescriptions].InnerText == "false")
                            isStandard = false;

                        properties.Add(new IOComponentProperty(key, value, isStandard));
                    }

                    // Read connections
                    Dictionary<string, string> connections = new Dictionary<string, string>();
                    XmlNodeList connectionNodes = element.SelectNodes("cdd:cns/cdd:cn", namespaceManager);
                    foreach (XmlNode connection in connectionNodes)
                    {
                        // Read connection
                        string point = connection.Attributes["pt"].InnerText;
                        string connectionId = connection.Attributes["id"].InnerText;

                        connections.Add(point, connectionId);
                    }

                    // Find type
                    IOComponentType type = null;
                    if (typeId.StartsWith("{") && typeId.EndsWith("}"))
                    {
                        // Type in expected format: {0}
                        string typeIdOnly = typeId.Substring(1, typeId.Length - 2); // Remove curly braces
                        if (componentTypes.ContainsKey(typeIdOnly))
                            type = componentTypes[typeIdOnly];
                        else
                            throw new NotSupportedException(); // Undefined type
                    }

                    Document.Components.Add(new IOComponent(id, location, size, flipped, orientation, type, properties, connections));
                }

                return true;
            }
        }

        /// <summary>
        /// Reads the metadata parts of a CDDX file.
        /// </summary>
        void ReadMetadata()
        {
            // Read core properties
            PackageRelationship coreRelationship = m_package.GetRelationshipsByType(RelationshipTypes.CoreProperties).FirstOrDefault();
            if (coreRelationship != null)
            {
                PackagePart corePart = m_package.GetPart(coreRelationship.TargetUri);
                try
                {
                    using (Stream coreStream = corePart.GetStream(FileMode.Open))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(coreStream);

                        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                        namespaceManager.AddNamespace("cp", "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/metadata/core-properties");
                        namespaceManager.AddNamespace("dc", Namespaces.DublinCore);

                        XmlNode creator = doc.SelectSingleNode("cp:coreProperties/dc:creator", namespaceManager);
                        if (creator != null)
                            Document.Metadata.Creator = creator.InnerText;
                        XmlNode date = doc.SelectSingleNode("cp:coreProperties/dc:date", namespaceManager);
                        if (date != null)
                        {
                            DateTime dateTime;
                            if (DateTime.TryParse(date.InnerText, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out dateTime))
                                Document.Metadata.Created = dateTime;
                        }
                        XmlNode description = doc.SelectSingleNode("cp:coreProperties/dc:description", namespaceManager);
                        if (description != null)
                            Document.Metadata.Description = description.InnerText;
                        XmlNode title = doc.SelectSingleNode("cp:coreProperties/dc:title", namespaceManager);
                        if (title != null)
                            Document.Metadata.Title = title.InnerText;
                    }
                }
                catch { }
            }

            // Read extended properties
            PackageRelationship extendedRelationship = m_package.GetRelationshipsByType(RelationshipTypes.ExtendedProperties).FirstOrDefault();
            if (extendedRelationship != null)
            {
                PackagePart extendedPart = m_package.GetPart(extendedRelationship.TargetUri);
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
                            Document.Metadata.Application = application.InnerText;
                        XmlNode appVersion = doc.SelectSingleNode("cp:extendedProperties/cp:appVersion", namespaceManager);
                        if (appVersion != null)
                            Document.Metadata.AppVersion = appVersion.InnerText;
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Reads information about the format version in use.
        /// </summary>
        private void ReadFormatProperties()
        {
            PackageRelationship formatPropertiesRelationship = m_package.GetRelationshipsByType(RelationshipTypes.FormatProperties).FirstOrDefault();
            if (formatPropertiesRelationship != null)
            {
                PackagePart extendedPart = m_package.GetPart(formatPropertiesRelationship.TargetUri);
                try
                {
                    using (Stream extendedStream = extendedPart.GetStream(FileMode.Open))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(extendedStream);

                        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                        namespaceManager.AddNamespace("fp", "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/format/properties");

                        XmlNode formatVersion = doc.SelectSingleNode("fp:formatProperties/fp:formatVersion", namespaceManager);
                        if (formatVersion != null)
                        {
                            double formatVersionValue = 1.0d;
                            if (double.TryParse(formatVersion.InnerText, out formatVersionValue) && formatVersionValue > FormatVersion)
                                m_newerVersion = true;
                        }
                    }
                }
                catch { }
            }
        }
    }
}
