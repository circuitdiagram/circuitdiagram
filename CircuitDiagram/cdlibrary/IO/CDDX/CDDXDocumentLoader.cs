// CDDXDocumentLoader.cs
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
using System.Xml;
using System.IO;
using System.IO.Packaging;
using CircuitDiagram.Components;
using CircuitDiagram.Elements;

namespace CircuitDiagram.IO.CDDX
{
    public static class CDDXDocumentLoader
    {
        private const string DocumentNamespace = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/document";

        public static DocumentLoadResult LoadCDDX(Package package, PackagePart documentPart, out CircuitDocument document)
        {
            double version = 1.0;

            try
            {
                using (Stream docStream = documentPart.GetStream(FileMode.Open))
                {
                    DocumentLoadResult loadResult = new DocumentLoadResult();

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

                    #region Definitions
                    // Component sources
                    XmlNodeList componentSourceNodes = doc.SelectNodes("/cdd:circuit/cdd:definitions/cdd:src", namespaceManager);
                    List<ComponentSourceLocation> componentSources = new List<ComponentSourceLocation>();

                    foreach (XmlElement element in componentSourceNodes)
                    {
                        string relationshipID = element.GetAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
                        string definitions = null;
                        if (element.HasAttribute("col"))
                            definitions = element.Attributes["col"].InnerText;


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
                            if (childElement.HasAttribute("impl"))
                                implements = childElement.Attributes["impl"].InnerText;

                            XmlNodeList configImplementationNodes = childElement.SelectNodes("cdd:conf", namespaceManager);
                            List<ComponentSource.ConfigurationImplementation> parsedImplementations = new List<ComponentSource.ConfigurationImplementation>();
                            foreach (XmlNode configImplementationNode in configImplementationNodes)
                                parsedImplementations.Add(new ComponentSource.ConfigurationImplementation(configImplementationNode.Attributes["name"].InnerText, configImplementationNode.Attributes["impl"].InnerText));

                            sources.Add(new ComponentSource(newSource, internalId, externalId, name, guid) { ConfigurationImplementations = parsedImplementations });
                        }

                        newSource.Sources = sources;
                        componentSources.Add(newSource);
                    }
                    #endregion

                    #region Document elements
                    // Document elements
                    XmlNodeList documentElements = doc.SelectNodes("/cdd:circuit/cdd:elements//cdd:c", namespaceManager);
                    foreach (XmlElement documentElement in documentElements)
                    {
                        if (documentElement == null)
                            continue;

                        string componentId = null;
                        if (documentElement.HasAttribute("id"))
                            componentId = documentElement.Attributes["id"].InnerText;
                        string componentType = null;
                        if (documentElement.HasAttribute("tp"))
                            componentType = documentElement.Attributes["tp"].InnerText;

                        // Load properties
                        Dictionary<string, object> properties = new Dictionary<string, object>();
                        XmlNodeList propertyNodes = documentElement.SelectNodes("cdd:prs/cdd:p", namespaceManager);
                        foreach (XmlElement propertyNode in propertyNodes)
                            properties.Add(propertyNode.Attributes["k"].InnerText, propertyNode.Attributes["v"].InnerText);

                        ComponentIdentifier comIdentifier = null;
                        comIdentifier = GetRepresentation(documentPart, componentSources, componentType);
                        if (comIdentifier != null)
                        {
                            if (!documentElement.HasAttribute("x") || !documentElement.HasAttribute("y"))
                            {
                                // No layout information
                                loadResult.Type = DocumentLoadResultType.FailIncorrectFormat;
                                loadResult.Errors.Add("The file contains no layout information.");
                                return loadResult;
                            }

                            double x = double.Parse(documentElement.Attributes["x"].InnerText);
                            double y = double.Parse(documentElement.Attributes["y"].InnerText);
                            bool horizontal = false;
                            if (documentElement.HasAttribute("o") && documentElement.Attributes["o"].InnerText.ToLowerInvariant() == "h")
                                horizontal = true;
                            double size = ComponentHelper.GridSize;
                            if (documentElement.HasAttribute("sz"))
                                size = double.Parse(documentElement.Attributes["sz"].InnerText);
                            bool flipped = false;
                            if (documentElement.HasAttribute("flp") && documentElement.Attributes["flp"].InnerText.ToLowerInvariant() == "true")
                                flipped = true;

                            Component component = Component.Create(comIdentifier, properties);
                            component.Layout(x, y, size, horizontal, flipped);
                            component.ApplyConnections(document);
                            document.Elements.Add(component);
                        }
                        else
                        {
                            // Description not found
                            ComponentSource location = FindComponentSource(componentSources, componentType);
                            if (location != null)
                            {


                                //if (!String.IsNullOrEmpty(location.ConfigurationImplementations))
                                //    unavailableComponents.Add(new StandardComponentRef(location.ConfigurationImplementations, location.ImplementationName));
                                //unavailableComponents.Add(new StandardComponentRef(location., location.ComponentName));
                            }
                            else
                            {
                                // ERROR: Undefined type
                                //unavailableComponents.Add(new UnavailableComponent(null, "Undefined type: " + componentType);
                            }
                        }
                    }

                    // Wires
                    XmlNodeList wires = doc.SelectNodes("/cdd:circuit/cdd:elements//cdd:w", namespaceManager);
                    foreach (XmlElement wire in wires)
                    {
                        double x = double.Parse(wire.Attributes["x"].InnerText);
                        double y = double.Parse(wire.Attributes["y"].InnerText);
                        bool horizontal = false;
                        if (wire.HasAttribute("o") && wire.Attributes["o"].InnerText.ToLowerInvariant() == "h")
                            horizontal = true;
                        double size = ComponentHelper.GridSize;
                        if (wire.HasAttribute("sz"))
                            size = double.Parse(wire.Attributes["sz"].InnerText);

                        Dictionary<string, object> properties = new Dictionary<string, object>(4);
                        properties.Add("@x", x);
                        properties.Add("@y", y);
                        properties.Add("@orientation", horizontal);
                        properties.Add("@size", size);

                        if (ComponentHelper.WireDescription != null)
                        {
                            Component wireComponent = Component.Create(ComponentHelper.WireDescription, properties);
                            wireComponent.Layout(x, y, size, horizontal, false);
                            wireComponent.ApplyConnections(document);
                            document.Elements.Add(wireComponent);
                        }
                    }
                    #endregion

                    document.Size = new System.Windows.Size(width, height);

                    if (version > CDDX.CDDXDocumentWriter.CDDXDocumentVersion)
                        loadResult.Type = DocumentLoadResultType.SuccessNewerVersion;
                    else
                        loadResult.Type = DocumentLoadResultType.Success;

                    return loadResult;
                }
            }
            catch (Exception)
            {
                document = null;

                DocumentLoadResult loadResult = new DocumentLoadResult();

                if (version > CDDX.CDDXDocumentWriter.CDDXDocumentVersion)
                    loadResult.Type = DocumentLoadResultType.FailNewerVersion;
                else
                    loadResult.Type = DocumentLoadResultType.FailUnknown;

                return loadResult;
            }
        }

        private static ComponentIdentifier GetRepresentation(PackagePart documentPart, List<ComponentSourceLocation> componentSources, string componentType)
        {
            if (componentType.StartsWith("{"))
            {
                // Find ComponentSource
                ComponentSource theSource = FindComponentSource(componentSources, componentType);

                if (theSource.Description != null)
                    return new ComponentIdentifier(theSource.Description);
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

                        return new ComponentIdentifier(theSource.Description);
                    }
                    else
                    {
                        // Not embedded in document
                        ComponentIdentifier result = null;

                        // Find by guid
                        if (theSource.GUID != Guid.Empty)
                            result = new ComponentIdentifier(ComponentHelper.FindDescription(theSource.GUID));
                        // Find by implementation for whole component
                        if (result == null && !String.IsNullOrEmpty(theSource.Location.DefinitionSource) && !String.IsNullOrEmpty(theSource.ImplementationName))
                            result = ComponentHelper.GetStandardComponent(theSource.Location.DefinitionSource, theSource.ImplementationName);
                        // Find by implementation for configuration
                        if (result == null)
                        {
                            // Create subset description for implementation only
                            return null;
                        }
                        // Find by component name
                        if (result == null)
                            result = new ComponentIdentifier(ComponentHelper.FindDescription(theSource.ComponentName));

                        return result;
                    }
                }
            }
            else
            {
                // Find by component name
                return new ComponentIdentifier(ComponentHelper.FindDescription(componentType));
            }
        }

        private static ComponentSource FindComponentSource(List<ComponentSourceLocation> componentSources, string componentType)
        {
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
            return theSource;
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
    }
}
