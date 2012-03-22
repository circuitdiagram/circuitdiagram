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
        public CircuitDocument Document { get; private set; }

        public DocumentLoadResult Load(Stream stream)
        {
            try
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

                DocumentLoadResult loadResult = new DocumentLoadResult();
                loadResult.Type = DocumentLoadResultType.FailNewerVersion;
                return loadResult;
            }
            catch (Exception)
            {
                DocumentLoadResult loadResult = new DocumentLoadResult();
                loadResult.Type = DocumentLoadResultType.FailIncorrectFormat;
                return loadResult;
            }
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
            return null;// new DocumentLoadResult(DocumentLoadResultType.Success);
        }

        private DocumentLoadResult LoadVersion1_1(XmlDocument doc)
        {
            try
            {
                DocumentLoadResult loadResult = new DocumentLoadResult();

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
                    Guid guid = Guid.Empty;
                    if ((node as XmlElement).HasAttribute("guid"))
                        guid = new Guid(node.Attributes["guid"].InnerText);

                    // Other properties
                    Dictionary<string, object> properties = new Dictionary<string, object>(node.Attributes.Count);
                    foreach (XmlAttribute attribute in node.Attributes)
                        properties.Add(attribute.Name, attribute.InnerText);

                    ComponentDescription description;
                    if (guid != Guid.Empty)
                    {
                        description = ComponentHelper.FindDescription(guid);

                        Component component = Component.Create(description, properties);
                        component.Offset = new System.Windows.Vector(x, y);
                        component.Horizontal = horizontal;
                        component.Size = size;

                        component.ResetConnections();

                        Document.Elements.Add(component);
                    }
                    else
                    {
                        string lType = type.ToLowerInvariant();
                        string standardCollection = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/components/common";

                        // Read type parameter
                        string t = null;
                        if ((node as XmlElement).HasAttribute("t"))
                            t = node.Attributes["t"].InnerText;
                        if (t == null && lType == "logicgate" && (node as XmlElement).HasAttribute("logictype"))
                            t = node.Attributes["logictype"].InnerText;

                        string standardComponentName = lType;

                        if (properties.ContainsKey("t"))
                            properties.Remove("t");
                        if (properties.ContainsKey("flipped"))
                        {
                            bool flipped = bool.Parse(properties["flipped"].ToString());
                            properties.Remove("flipped");
                            properties.Add("@flipped", flipped);
                        }

                        if (lType == "logicgate" && t == "0")
                            standardComponentName = "and2";
                        else if (lType == "logicgate" && t == "1")
                            standardComponentName = "nand2";
                        else if (lType == "logicgate" && t == "2")
                            standardComponentName = "or2";
                        else if (lType == "logicgate" && t == "3")
                            standardComponentName = "nor2";
                        else if (lType == "logicgate" && t == "4")
                            standardComponentName = "xor2";
                        else if (lType == "logicgate" && t == "5")
                            standardComponentName = "not";
                        else if (lType == "logicgate" && t == "6")
                            standardComponentName = "schmittnot";
                        else if (lType == "supply")
                            standardComponentName = "cell";
                        else if (lType == "switch" && t == "0")
                            standardComponentName = "pushswitch";
                        else if (lType == "switch" && t == "1")
                            standardComponentName = "toggleswitch";
                        else if (lType == "transistor" && t == "0")
                            standardComponentName = "mosfetn";
                        else if (lType == "transistor" && t == "1")
                            standardComponentName = "mosfetp";
                        else if (lType == "transistor" && t == "2")
                            standardComponentName = "transnpn";
                        else if (lType == "transistor" && t == "3")
                            standardComponentName = "transpnp";
                        else if (lType == "outputdevice" && t == "3")
                            standardComponentName = "heater";

                        ComponentIdentifier identifier = ComponentHelper.GetStandardComponent(standardCollection, standardComponentName);
                        if (lType == "wire")
                            identifier = new ComponentIdentifier(ComponentHelper.WireDescription);

                        if (identifier != null)
                        {
                            Component component = Component.Create(identifier, properties);
                            component.Offset = new System.Windows.Vector(x, y);
                            component.Horizontal = horizontal;
                            component.Size = size;
                            component.ResetConnections();

                            Document.Elements.Add(component);
                        }
                        else
                        {
                            // Unknown component
                            if (standardComponentName == lType)
                                loadResult.UnavailableComponents.Add(new StandardComponentRef(null, type)); // Unknown type
                            else
                                loadResult.UnavailableComponents.Add(new StandardComponentRef(standardCollection, standardComponentName)); // No implementation for known type
                        }
                    }
                }

                foreach (Component component in Document.Components)
                    component.ApplyConnections(Document);

                loadResult.Type = DocumentLoadResultType.Success;
                return loadResult;
            }
            catch (Exception)
            {
                // Wrong format
                DocumentLoadResult loadResult = new DocumentLoadResult();
                loadResult.Type = DocumentLoadResultType.FailIncorrectFormat;
                return loadResult;
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
    }
}
