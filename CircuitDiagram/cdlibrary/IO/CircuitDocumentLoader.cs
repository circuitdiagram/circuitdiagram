// CircuitDocumentLoader.cs
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
using CircuitDiagram.Components;
using System.Net;
using System.IO.Packaging;
using CircuitDiagram.Elements;

namespace CircuitDiagram.IO
{
    public enum CircuitDocumentXmlVersion
    {
        Unknown = 0,
        Version1_0 = 1,
        Version1_1 = 2,
        Version1_2 = 3
    }

    public class CircuitDocumentLoader
    {
        public CircuitDocumentLoader()
        {
        }

        public CircuitDocument Document { get; private set; }

        public static DocumentLoadResult LoadCDDX(Package package, PackagePart documentPart, out CircuitDocument document)
        {
            double version = 1.0;

            try
            {
                using (Stream docStream = documentPart.GetStream(FileMode.Open))
                {
                    document = new CircuitDocument();

                    XmlDocument doc = new XmlDocument();
                    doc.Load(docStream);

                    XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                    namespaceManager.AddNamespace("cdd", "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/document");

                    double.TryParse(doc.SelectSingleNode("/cdd:circuit", namespaceManager).Attributes["version"].InnerText, out version);

                    #region Metadata
                    // Metadata
                    double width = 640d;
                    double height = 480d;
                    XmlNodeList metadataNodes = doc.SelectNodes("/cdd:circuit/cdd:metadata/cdd:property", namespaceManager);
                    foreach (XmlNode metadataNode in metadataNodes)
                    {
                        string propertyName = metadataNode.Attributes["name"].InnerText.ToLowerInvariant();
                        string propertyValue = metadataNode.Attributes["value"].InnerText;

                        if (propertyName == "width")
                            width = double.Parse(propertyValue);
                        else if (propertyName == "height")
                            height = double.Parse(propertyValue);
                    }
                    #endregion

                    #region Component sources
                    // Component sources
                    XmlNodeList componentSourceNodes = doc.SelectNodes("/cdd:circuit/cdd:components/cdd:source", namespaceManager);
                    List<ComponentSourceLocation> componentSources = new List<ComponentSourceLocation>();

                    foreach (XmlElement element in componentSourceNodes)
                    {
                        string relationshipID = element.GetAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
                        string definitions = null;
                        if (element.HasAttribute("definitions"))
                            definitions = element.Attributes["definitions"].InnerText;


                        ComponentSourceLocation newSource = new ComponentSourceLocation(relationshipID, definitions);

                        List<ComponentSource> sources = new List<ComponentSource>();
                        foreach (XmlElement childElement in element.SelectNodes("cdd:add", namespaceManager))
                        {

                            string internalId = childElement.Attributes["id"].InnerText;
                            string externalId = null;
                            if (childElement.HasAttribute("xid"))
                                externalId = childElement.Attributes["xid"].InnerText;
                            string name = null;
                            if (childElement.HasAttribute("name"))
                                name = childElement.Attributes["name"].InnerText;
                            Guid guid = Guid.Empty;
                            if (childElement.HasAttribute("guid"))
                                guid = new Guid(childElement.Attributes["guid"].InnerText);
                            string implements = null;
                            if (childElement.HasAttribute("implements"))
                                implements = childElement.Attributes["implements"].InnerText;

                            XmlNodeList configImplementationNodes = childElement.SelectNodes("cdd:configuration", namespaceManager);
                            List<ComponentSource.ConfigurationImplementation> parsedImplementations = new List<ComponentSource.ConfigurationImplementation>();
                            foreach (XmlNode configImplementationNode in configImplementationNodes)
                                parsedImplementations.Add(new ComponentSource.ConfigurationImplementation(configImplementationNode.Attributes["name"].InnerText, configImplementationNode.Attributes["implements"].InnerText));

                            sources.Add(new ComponentSource(newSource, internalId, externalId, name, guid) { ConfigurationImplementations = parsedImplementations });
                        }

                        newSource.Sources = sources;
                        componentSources.Add(newSource);
                    }
                    #endregion

                    #region Document elements
                    // Document elements
                    Dictionary<string, ICircuitElement> circuitElements = new Dictionary<string, ICircuitElement>();
                    XmlNodeList documentElements = doc.SelectNodes("/cdd:circuit/cdd:document/cdd:component", namespaceManager);
                    foreach (XmlElement documentElement in documentElements)
                    {
                        if (documentElement == null)
                            continue;

                        string componentId = null;
                        if (documentElement.HasAttribute("id"))
                            componentId = documentElement.Attributes["id"].InnerText;
                        string componentType = null;
                        if (documentElement.HasAttribute("type"))
                            componentType = documentElement.Attributes["type"].InnerText;

                        // Load properties
                        Dictionary<string, object> properties = new Dictionary<string, object>();
                        XmlNodeList propertyNodes = documentElement.SelectNodes("cdd:properties/cdd:property", namespaceManager);
                        foreach (XmlElement propertyNode in propertyNodes)
                            properties.Add(propertyNode.Attributes["key"].InnerText, propertyNode.Attributes["value"].InnerText);

                        ComponentDescription description = null;
                        description = GetDescription(documentPart, componentSources, componentType);
                        if (description != null)
                            circuitElements.Add(componentId, Component.Create(description, properties));
                        else
                        {
                            // Description not found
                        }
                    }
                    #endregion

                    #region Layout
                    // Layout
                    XmlNode parentLayoutNode = doc.SelectSingleNode("/cdd:circuit/cdd:layout", namespaceManager);
                    foreach (XmlElement layoutNode in parentLayoutNode.ChildNodes)
                    {
                        if (layoutNode.Name == "component")
                        {
                            string id = layoutNode.Attributes["id"].InnerText;
                            double x = double.Parse(layoutNode.Attributes["x"].InnerText);
                            double y = double.Parse(layoutNode.Attributes["y"].InnerText);
                            bool horizontal = false;
                            if (layoutNode.HasAttribute("orientation") && layoutNode.Attributes["orientation"].InnerText.ToLowerInvariant() == "horizontal")
                                horizontal = true;
                            double size = ComponentHelper.GridSize;
                            if (layoutNode.HasAttribute("size"))
                                size = double.Parse(layoutNode.Attributes["size"].InnerText);
                            bool flipped = false;
                            if (layoutNode.HasAttribute("flipped") && layoutNode.Attributes["flipped"].InnerText.ToLowerInvariant() == "true")
                                flipped = true;

                            if (circuitElements.ContainsKey(id))
                            {
                                Component component = circuitElements[id] as Component;
                                if (component != null)
                                {
                                    component.Layout(x, y, size, horizontal, flipped);
                                    component.ApplyConnections(document);
                                }
                                document.Elements.Add(component);
                            }
                            else
                            {
                                // Something's not right...
                            }
                        }
                        else if (layoutNode.Name == "wire")
                        {
                            double x = double.Parse(layoutNode.Attributes["x"].InnerText);
                            double y = double.Parse(layoutNode.Attributes["y"].InnerText);
                            bool horizontal = false;
                            if (layoutNode.HasAttribute("orientation") && layoutNode.Attributes["orientation"].InnerText.ToLowerInvariant() == "horizontal")
                                horizontal = true;
                            double size = ComponentHelper.GridSize;
                            if (layoutNode.HasAttribute("size"))
                                size = double.Parse(layoutNode.Attributes["size"].InnerText);

                            Dictionary<string, object> properties = new Dictionary<string,object>(4);
                            properties.Add("@x", x);
                            properties.Add("@y", y);
                            properties.Add("@horizontal", horizontal);
                            properties.Add("@size", size);

                            if (ComponentHelper.WireDescription != null)
                            {
                                Component wire = Component.Create(ComponentHelper.WireDescription, properties);
                                document.Elements.Add(wire);
                            }
                        }
                    }

                    // Add components which did not have layout information (although they ALL either should or shouldn't...)
                    foreach (ICircuitElement element in circuitElements.Values)
                    {
                        if (!document.Elements.Contains(element))
                        {
                            throw new NotImplementedException();
                        }
                    }
                    #endregion

                    document.Size = new System.Windows.Size(width, height);

                    if (version > CircuitDocumentWriter.CDDXDocumentVersion)
                        return DocumentLoadResult.SuccessNewerVersion;
                    else
                        return DocumentLoadResult.Success;
                }
            }
            catch (Exception)
            {
                document = null;
                if (version > CircuitDocumentWriter.CDDXDocumentVersion)
                    return DocumentLoadResult.FailNewerVersion;
                else
                    return DocumentLoadResult.FailUnknown;
            }
        }

        private static ComponentDescription GetDescription(PackagePart documentPart, List<ComponentSourceLocation> componentSources, string componentType)
        {
            if (componentType.StartsWith("{"))
            {
                // Find ComponentSourceLocation
                string componentTypeID = componentType.Replace("{", "").Replace("}", "");
                ComponentSource theSource = null;
                foreach (ComponentSourceLocation searchLocation in componentSources)
                {
                    ComponentSource source = searchLocation.Sources.FirstOrDefault(item => item.InternalID == componentTypeID);
                    if (source != null)
                    {
                        theSource = source;
                        break;
                    }
                }

                if (theSource.Description != null)
                    return theSource.Description;
                else
                {
                    // Load description
                    if (!String.IsNullOrEmpty(theSource.Location.RelationshipID))
                    {
                        // Embedded in document
                        PackageRelationship relationship = documentPart.GetRelationship(theSource.Location.RelationshipID);
                        PackagePart descriptionFilePart = relationship.Package.GetPart(PackUriHelper.ResolvePartUri(documentPart.Uri, relationship.TargetUri));

                        ComponentDescription[] descriptions = null;
                        using (var stream = descriptionFilePart.GetStream())
                        {
                            if (descriptionFilePart.ContentType == ContentTypes.BinaryComponent)
                            {
                                // Binary component
                                BinaryLoader loader = new BinaryLoader();
                                loader.Load(stream);
                                descriptions = loader.GetDescriptions();
                            }
                            else if (descriptionFilePart.ContentType == System.Net.Mime.MediaTypeNames.Text.Xml)
                            {
                                // XML component
                                XmlLoader loader = new XmlLoader();
                                loader.Load(stream);
                                descriptions = loader.GetDescriptions();
                            }
                            else
                            {
                                // Unsupported format
                            }
                        }

                        if (descriptions != null)
                        {
                            if (descriptions.Length == 1)
                                theSource.Description = descriptions[0];
                            else
                            {
                                foreach (ComponentDescription description in descriptions)
                                {
                                    ComponentSource matchingSource = theSource.Location.Sources.FirstOrDefault(item => item.ExternalID == description.ID);
                                    if (matchingSource != null)
                                        matchingSource.Description = description;
                                }
                            }

                            // Add to ComponentHelper so can be used by UndoManager
                            foreach (ComponentDescription description in descriptions)
                                ComponentHelper.AddDescription(description);
                        }

                        return theSource.Description;
                    }
                    else
                    {
                        // Not embedded in document
                        ComponentDescription description = null;

                        // Find by guid
                        if (theSource.GUID != Guid.Empty)
                            description = ComponentHelper.FindDescription(theSource.GUID);
                        // Find by implementation for whole component
                        if (description == null && !String.IsNullOrEmpty(theSource.Location.DefinitionSource) && !String.IsNullOrEmpty(theSource.ImplementationName))
                            description = ComponentHelper.FindDescription(theSource.Location.DefinitionSource, theSource.ImplementationName);
                        // Find by implementation for configuration
                        if (description == null)
                        {
                            // Create subset description for implementation only
                            throw new NotImplementedException();
                        }
                        // Find by component name
                        if (description == null)
                            description = ComponentHelper.FindDescription(theSource.ComponentName);

                        return description;
                    }
                }
            }
            else
            {
                // Find by component name
                return ComponentHelper.FindDescription(componentType);
            }
        }

        class ComponentSource
        {
            public ComponentSourceLocation Location { get; set; }
            public string InternalID { get; set; }
            public string ExternalID { get; set; }
            public string ComponentName { get; set; }
            public Guid GUID { get; set; }

            public string ImplementationName { get; set; }
            public List<ConfigurationImplementation> ConfigurationImplementations { get; set; }

            public ComponentDescription Description { get; set; }

            public ComponentSource(ComponentSourceLocation location, string internalID, string externalID, string componentName, Guid guid)
            {
                Location = location;
                InternalID = internalID;
                ExternalID = externalID;
                ComponentName = componentName;
                GUID = guid;
            }

            public class ConfigurationImplementation
            {
                public string ImplementationName { get; set; }
                public string ConfigurationName { get; set; }

                public ConfigurationImplementation(string implementationName, string configurationName)
                {
                    ImplementationName = implementationName;
                    ConfigurationName = configurationName;
                }
            }
        }

        class ComponentSourceLocation
        {
            public string RelationshipID { get; private set; }
            public string DefinitionSource { get; private set; }
            public List<ComponentSource> Sources { get; set; }

            public ComponentSourceLocation(string relationshipID, string definitionSource)
            {
                RelationshipID = relationshipID;
                DefinitionSource = definitionSource;
            }
        }

        public DocumentLoadResult Load(Stream stream)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(stream);

            CircuitDocumentXmlVersion version = CircuitDocumentXmlVersion.Unknown;
            if ((doc.SelectSingleNode("/circuit") as XmlElement).HasAttribute("version"))
            {
                string versionString = doc.SelectSingleNode("/circuit").Attributes["version"].InnerText;
                double versionDouble;
                if (double.TryParse(versionString, out versionDouble))
                {
                    if (versionDouble == 1.0d)
                        version = CircuitDocumentXmlVersion.Version1_0;
                    else if (versionDouble == 1.1d)
                        version = CircuitDocumentXmlVersion.Version1_1;
                    else if (versionDouble == 1.2d)
                        version = CircuitDocumentXmlVersion.Version1_2;
                }
            }

            if (version == CircuitDocumentXmlVersion.Version1_2)
                return LoadVersion1_2(doc);
            else if (version == CircuitDocumentXmlVersion.Version1_1)
                return LoadVersion1_1(doc);
            else if (version == CircuitDocumentXmlVersion.Version1_0)
                return LoadVersion1_0(doc);

            return DocumentLoadResult.FailNewerVersion;
        }

        private DocumentLoadResult LoadVersion1_0(XmlDocument doc)
        {
            

            //XmlTextReader reader = new XmlTextReader(stream);
            //this.Components.Clear();

            //bool errorOccurred = false;
            //while (reader.Read())
            //{
            //    if (reader.NodeType == XmlNodeType.Element && reader.Depth == 0)
            //    {
            //        try
            //        {
            //            reader.MoveToAttribute("width");
            //            Size.Width = double.Parse(reader.Value);
            //            reader.MoveToAttribute("height");
            //            Size.Height = double.Parse(reader.Value);
            //            reader.MoveToElement();
            //        }
            //        catch (Exception)
            //        {
            //            errorOccurred = true;
            //        }
            //    }
            //    else if (reader.NodeType == XmlNodeType.Element && reader.Depth == 1 && reader.LocalName == "component")
            //    {
            //        try
            //        {
            //            reader.MoveToAttribute("type");
            //            string componentType = reader.Value;
            //            reader.MoveToElement();
            //            Dictionary<string, object> properties = new Dictionary<string, object>();
            //            reader.MoveToNextAttribute();
            //            for (int i = 0; i < reader.AttributeCount; i++)
            //            {
            //                properties.Add(reader.Name, reader.Value);
            //                reader.MoveToNextAttribute();
            //            }
            //            Component component = Component.Create(ComponentDataString.ConvertToString(properties));
            //            reader.MoveToElement();

            //            m_components.Add(component);
            //        }
            //        catch (Exception)
            //        {
            //            errorOccurred = true;
            //        }
            //    }
            //}
            //reader.Close();

            //foreach (Component component in Components)
            //    component.ApplyConnections(this);

            //if (errorOccurred)
            //    return DocumentLoadResult.FailIncorrectFormat;
            //else
            return DocumentLoadResult.Success;
        }

        private DocumentLoadResult LoadVersion1_1(XmlDocument doc)
        {
            try
            {
                Document = new CircuitDocument();

                XmlElement circuitNode = doc.SelectSingleNode("/circuit") as XmlElement;
                if (circuitNode.HasAttribute("width") && circuitNode.HasAttribute("height"))
                    Document.Size = new System.Windows.Size(double.Parse(circuitNode.Attributes["width"].InnerText), double.Parse(circuitNode.Attributes["height"].InnerText));
    
                XmlNodeList componentNodes = doc.SelectNodes("/circuit/component");
                foreach (XmlNode node in componentNodes)
                {
                    string type = node.Attributes["type"].InnerText;
                    double x = double.Parse(node.Attributes["x"].InnerText);
                    double y = double.Parse(node.Attributes["y"].InnerText);
                    double size = ComponentHelper.GridSize;
                    if ((node as XmlElement).HasAttribute("size"))
                        size = double.Parse(node.Attributes["size"].InnerText);
                    bool horizontal = true;
                    if ((node as XmlElement).HasAttribute("orientation") && node.Attributes["orientation"].InnerText.ToLowerInvariant() == "vertical")
                        horizontal = false;

                    // Other properties
                    Dictionary<string, object> properties = new Dictionary<string, object>(node.Attributes.Count);
                    foreach (XmlAttribute attribute in node.Attributes)
                        properties.Add(attribute.Name, attribute.InnerText);

                    ComponentDescription description = ComponentHelper.FindDescription(type);
                    if (description != null)
                    {
                        Component component = Component.Create(description, properties);
                        component.Offset = new System.Windows.Vector(x, y);
                        component.Horizontal = horizontal;
                        component.Size = size;

                        component.ResetConnections();

                        Document.Elements.Add(component);
                    }
                }

                foreach (Component component in Document.Components)
                    component.ApplyConnections(Document);

                return DocumentLoadResult.Success;
            }
            catch (Exception)
            {
                // Wrong format
                return DocumentLoadResult.FailIncorrectFormat;
            }
        }

        private DocumentLoadResult LoadVersion1_2(XmlDocument doc)
        {
            throw new NotImplementedException();

            //try
            //{
            //    m_document = new CircuitDocument();

            //    // Root properties
            //    XmlElement circuitElement = doc.SelectSingleNode("/circuit") as XmlElement;

            //    string width = circuitElement.Attributes["width"].InnerText;
            //    string height = circuitElement.Attributes["height"].InnerText;

            //    // Metadata
            //    XmlElement metadataElement = doc.SelectSingleNode("/circuit/metadata") as XmlElement;

            //    Dictionary<string, string> metadata = new Dictionary<string, string>();
            //    foreach (XmlNode metadataItem in metadataElement.ChildNodes)
            //        if (!metadata.ContainsKey(metadataItem.Name))
            //            metadata.Add(metadataItem.Name, metadataItem.InnerText);

            //    // Component sources
            //    XmlNodeList componentSourceNodes = doc.SelectNodes("/circuit/components/source");
            //    Dictionary<string, ComponentSource> componentSources = new Dictionary<string, ComponentSource>();

            //    foreach (XmlElement element in componentSourceNodes)
            //    {
            //        string location = element.Attributes["location"].InnerText;

            //        foreach (XmlElement childElement in element.SelectNodes("add"))
            //        {
            //            string internalId = childElement.Attributes["id"].InnerText;
            //            string externalId = null;
            //            if (childElement.HasAttribute("xid"))
            //                externalId = childElement.Attributes["xid"].InnerText;
            //            string name = null;
            //            if (childElement.HasAttribute("name"))
            //                name = childElement.Attributes["name"].InnerText;
            //            Guid guid = Guid.Empty;
            //            if (childElement.HasAttribute("guid"))
            //                guid = new Guid(childElement.Attributes["guid"].InnerText);
            //            componentSources.Add(internalId, new ComponentSource(location, internalId, externalId, name, guid));
            //        }
            //    }

            //    // Document elements
            //    XmlNodeList elementNodes = doc.SelectNodes("/circuit/document/element");
            //    foreach (XmlNode componentNode in elementNodes)
            //    {
            //        string type = componentNode.Attributes["type"].InnerText;
            //        double x = double.Parse(componentNode.Attributes["x"].InnerText);
            //        double y = double.Parse(componentNode.Attributes["y"].InnerText);
            //        string orientation = componentNode.Attributes["orientation"].InnerText;

            //        Dictionary<string, object> componentProperties = new Dictionary<string, object>();
            //        componentProperties.Add("@x", x);
            //        componentProperties.Add("@y", y);
            //        componentProperties.Add("@orientation", orientation);

            //        XmlNodeList componentPropertyNodes = componentNode.SelectNodes("property");
            //        foreach (XmlNode propertyNode in componentPropertyNodes)
            //        {
            //            string key = propertyNode.Attributes["key"].InnerText;
            //            string value = propertyNode.Attributes["value"].InnerText;
            //            componentProperties.Add(key, value);
            //        }

            //        ComponentDescription componentDescription;
            //        if (!type.StartsWith("{"))
            //        {
            //            componentDescription = ComponentHelper.FindDescription(type);
            //        }
            //        else
            //        {
            //            if (componentSources.ContainsKey(type.Replace("{", "").Replace("}", "")))
            //            {
            //                ComponentSource source = componentSources[type.Replace("{", "").Replace("}", "")];
            //                //if (!source.IsResolved)
            //                //    source.Resolve(rc);
            //                componentDescription = source.Value;
            //            }
            //            else
            //            {
            //                // Invalid component reference
            //                componentDescription = null;
            //            }
            //        }

            //        if (componentDescription.CanResize && ((XmlElement)componentNode).HasAttribute("size"))
            //            componentProperties.Add("@size", double.Parse(componentNode.Attributes["size"].InnerText));
            //        else if (componentDescription.CanResize)
            //            componentProperties.Add("@size", ComponentHelper.GridSize);
            //        if (componentDescription.CanFlip && ((XmlElement)componentNode).HasAttribute("flipped") && componentNode.Attributes["flipped"].InnerText.ToLower() == "true")
            //            componentProperties.Add("@flipped", true);
            //        else if (componentDescription.CanFlip)
            //            componentProperties.Add("@flipped", false);

            //        m_document.Elements.Add(Component.Create(componentDescription, componentProperties));
            //    }

            //    return DocumentLoadResult.Success;
            //}
            //catch (Exception)
            //{
            //    return DocumentLoadResult.FailUnknown;
            //}
        }

        /*class ComponentSource
        {
            public string Location { get; set; }
            public string InternalID { get; set; }
            public string ExternalID { get; set; }
            public string Name { get; set; }
            public Guid GUID { get; set; }
            public bool IsResolved { get { return Value != null; } }
            public ComponentDescription Value { get; private set; }

            public ComponentSource(string location, string internalId, string externalId, string name, Guid guid)
            {
                Location = location;
                InternalID = internalId;
                ExternalID = externalId;
                Name = name;
                GUID = guid;
            }

            public void Resolve(PackagePart documentPart)
            {
                // Check if component is already available
                //if (GUID != Guid.Empty)
                //    Value = ComponentHelper.FindDescription(GUID);
                //else if (description == null && !String.IsNullOrEmpty(Name))
                //    Value = ComponentHelper.FindDescription(Name);
                //else
                //{
                //    // Determine path type
                //    if (Location.StartsWith("http://"))
                //    {
                //        try
                //        {
                //            FileWebRequest webRequest = (FileWebRequest)WebRequest.Create(Location.Replace("%", ExternalID));
                //            WebResponse response = webRequest.GetResponse();
                //            Stream responseStream = response.GetResponseStream();
                //            if (response.ContentType == System.Net.Mime.MediaTypeNames.Text.Xml)
                //            {
                //                // XML format
                //                throw new NotImplementedException();
                //            }
                //            else
                //            {
                //                // Assume binary format
                //                throw new NotImplementedException();
                //            }
                //        }
                //        catch (WebException)
                //        {
                //            throw new NotImplementedException();
                //        }
                //    }
                //    else if (Location.StartsWith("r:"))
                //    {
                //        string relationshipId = Location.Replace("r:", "");
                //        PackageRelationship componentRelationship = documentPart.GetRelationship(relationshipId);
                //        PackagePart componentPart = documentPart.Package.GetPart(componentRelationship.TargetUri);

                //        if (componentPart.ContentType == System.Net.Mime.MediaTypeNames.Text.Xml)
                //        {
                //            using (Stream stream = componentPart.GetStream(FileMode.Open))
                //            {
                //                XmlLoader loader = new XmlLoader();
                //                loader.Load(stream);
                //                Value = loader.GetDescriptions().FirstOrDefault();
                //            }
                //        }
                //        else if (componentPart.ContentType == ContentTypes.BinaryComponent)
                //        {
                //            using (Stream stream = componentPart.GetStream(FileMode.Open))
                //            {
                //                BinaryLoader loader = new BinaryLoader();
                //                loader.Load(stream);
                //                descriptionResource.Deserialized = loader.GetDescriptions();
                //            }
                //        }
                //        else
                //        {
                //            // Unknown content type
                //        }

                //        if (rc.IsResourceAvailable(passLocation))
                //        {
                //            Resource descriptionResource = rc.GetResource(passLocation);

                //            if (descriptionResource.Deserialized == null)
                //            {
                //                if (descriptionResource.ContentType == "text/xml")
                //                {
                //                    using (MemoryStream stream = new MemoryStream(descriptionResource.Data))
                //                    {
                //                        XmlLoader loader = new XmlLoader();
                //                        loader.Load(stream);
                //                        descriptionResource.Deserialized = loader.GetDescriptions();
                //                    }
                //                }
                //                else
                //                {
                //                    // Assume packaged as binary (CDCOM)
                //                    using (MemoryStream stream = new MemoryStream(descriptionResource.Data))
                //                    {
                //                        BinaryLoader loader = new BinaryLoader();
                //                        loader.Load(stream);
                //                        descriptionResource.Deserialized = loader.GetDescriptions();
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                foreach (CircuitDiagram.Components.ComponentDescription description2 in (descriptionResource.Deserialized as ComponentDescription[]))
                //                    if (description.ID == InternalID)
                //                        description = description2;
                //            }

                //            ComponentHelper.AddDescription(description);
                //        }
                //        else
                //        {
                //            throw new NotImplementedException("The component description could not be found.");
                //        }
                //    }
                //}
                //Value = description;
            }
        }*/
    }
}
