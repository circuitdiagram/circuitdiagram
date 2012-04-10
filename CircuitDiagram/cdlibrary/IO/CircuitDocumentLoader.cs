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

    /// <summary>
    /// Loads circuit documents from XML format.
    /// </summary>
    public class CircuitDocumentLoader
    {
        /// <summary>
        /// The document loaded from the stream.
        /// </summary>
        public CircuitDocument Document { get; private set; }

        /// <summary>
        /// Loads a circuit document from XML format.
        /// </summary>
        /// <param name="stream">The stream containing a circuit document in XML format.</param>
        /// <returns>The load result.</returns>
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
                    }
                }

                if (version == CircuitDocumentXmlVersion.Version1_1)
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
            try
            {
                DocumentLoadResult loadResult = new DocumentLoadResult();

                Document = new CircuitDocument();

                XmlElement circuitNode = doc.SelectSingleNode("/circuit") as XmlElement;
                if (circuitNode.HasAttribute("width") && circuitNode.HasAttribute("height"))
                    Document.Size = new System.Windows.Size(double.Parse(circuitNode.Attributes["width"].InnerText), double.Parse(circuitNode.Attributes["height"].InnerText));
                string application = null;
                string appVersion = null;
                if (circuitNode.HasAttribute("application"))
                    application = circuitNode.Attributes["application"].InnerText;
                else if (circuitNode.HasAttribute("cd-version"))
                {
                    application = "Circuit Diagram ";
                    appVersion = circuitNode.Attributes["cd-version"].InnerText;
                }

                foreach (XmlNode node in circuitNode.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                        continue;

                    string type = node.Name;
                    string location = node.Attributes["location"].InnerText;
                    string[] locationSplit = location.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    // Snap points to grid
                    double x = Math.Round(double.Parse(locationSplit[0]) / ComponentHelper.GridSize) * ComponentHelper.GridSize;
                    double y = Math.Round(double.Parse(locationSplit[1]) / ComponentHelper.GridSize) * ComponentHelper.GridSize;
                    double endX = Math.Round(double.Parse(locationSplit[2]) / ComponentHelper.GridSize) * ComponentHelper.GridSize;
                    double endY = Math.Round(double.Parse(locationSplit[3]) / ComponentHelper.GridSize) * ComponentHelper.GridSize;
                    double size = endX - x;
                    bool horizontal = true;
                    if (endX == x)
                    {
                        size = endY - y;
                        horizontal = false;
                    }

                    // Other properties
                    Dictionary<string, object> properties = new Dictionary<string, object>(node.Attributes.Count);
                    foreach (XmlAttribute attribute in node.Attributes)
                        properties.Add(attribute.Name, attribute.InnerText);

                    string lType = type.ToLowerInvariant();

                    // Read type parameter
                    string t = null;
                    if ((node as XmlElement).HasAttribute("t"))
                        t = node.Attributes["t"].InnerText;
                    else if ((node as XmlElement).HasAttribute("type"))
                        t = node.Attributes["type"].InnerText;

                    if (properties.ContainsKey("t"))
                        properties.Remove("t");

                    // Convert component name
                    string componentCollection;
                    string standardComponentName;
                    ConvertComponentName(lType, t, out componentCollection, out standardComponentName);
                    ConvertProperties(componentCollection, standardComponentName, properties);

                    ComponentIdentifier identifier = null;
                    if (componentCollection != null)
                        identifier = ComponentHelper.GetStandardComponent(componentCollection, standardComponentName);
                    else
                        identifier = ComponentHelper.GetStandardComponent(CDDX.ComponentCollections.Standard, standardComponentName);
                    if (lType == "wire")
                        identifier = new ComponentIdentifier(ComponentHelper.WireDescription);

                    if (identifier != null)
                    {
                        Component component = Component.Create(identifier, properties);
                        component.Offset = new System.Windows.Vector(x, y);
                        component.Horizontal = horizontal;
                        if (size < component.Description.MinSize)
                            component.Size = component.Description.MinSize;
                        else
                            component.Size = size;
                        component.ResetConnections();

                        Document.Elements.Add(component);
                    }
                    else
                    {
                        // Unknown component
                        if (componentCollection == null)
                            loadResult.UnavailableComponents.Add(new StandardComponentRef(null, type)); // Unknown type
                        else
                            loadResult.UnavailableComponents.Add(new StandardComponentRef(componentCollection, standardComponentName)); // No implementation for known type
                    }
                }

                foreach (Component component in Document.Components)
                    component.ApplyConnections(Document);

                // Reset metadata
                Document.Metadata = new CircuitDocumentMetadata();
                Document.Metadata.Format = "XML (1.0)";
                Document.Metadata.Extended.Application = application;
                Document.Metadata.Extended.AppVersion = appVersion;

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

        private DocumentLoadResult LoadVersion1_1(XmlDocument doc)
        {
            try
            {
                DocumentLoadResult loadResult = new DocumentLoadResult();

                Document = new CircuitDocument();

                XmlElement circuitNode = doc.SelectSingleNode("/circuit") as XmlElement;
                if (circuitNode.HasAttribute("width") && circuitNode.HasAttribute("height"))
                    Document.Size = new System.Windows.Size(double.Parse(circuitNode.Attributes["width"].InnerText), double.Parse(circuitNode.Attributes["height"].InnerText));
                string application = null;
                string appVersion = null;
                if (circuitNode.HasAttribute("application"))
                    application = circuitNode.Attributes["application"].InnerText;
                else if (circuitNode.HasAttribute("cd-version"))
                {
                    application = "Circuit Diagram ";
                    appVersion = circuitNode.Attributes["cd-version"].InnerText;
                }

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

                        // Read type parameter
                        string t = null;
                        if ((node as XmlElement).HasAttribute("t"))
                            t = node.Attributes["t"].InnerText;
                        if (t == null && lType == "logicgate" && (node as XmlElement).HasAttribute("logictype"))
                            t = node.Attributes["logictype"].InnerText;

                        if (properties.ContainsKey("t"))
                            properties.Remove("t");
                        if (properties.ContainsKey("flipped"))
                        {
                            bool flipped = bool.Parse(properties["flipped"].ToString());
                            properties.Remove("flipped");
                            properties.Add("@flipped", flipped);
                        }

                        // Convert component name
                        string componentCollection;
                        string standardComponentName;
                        ConvertComponentName(lType, t, out componentCollection, out standardComponentName);
                        ConvertProperties(componentCollection, standardComponentName, properties);

                        ComponentIdentifier identifier = null;
                        if (componentCollection != null)
                            identifier = ComponentHelper.GetStandardComponent(componentCollection, standardComponentName);
                        else
                            identifier = ComponentHelper.GetStandardComponent(CDDX.ComponentCollections.Standard, standardComponentName);
                        if (lType == "wire")
                            identifier = new ComponentIdentifier(ComponentHelper.WireDescription);

                        if (identifier != null)
                        {
                            Component component = Component.Create(identifier, properties);
                            component.Offset = new System.Windows.Vector(x, y);
                            component.Horizontal = horizontal;
                            if (size < component.Description.MinSize)
                                component.Size = component.Description.MinSize;
                            else
                                component.Size = size;
                            component.ResetConnections();

                            Document.Elements.Add(component);
                        }
                        else
                        {
                            // Unknown component
                            if (componentCollection == null)
                                loadResult.UnavailableComponents.Add(new StandardComponentRef(null, type)); // Unknown type
                            else
                                loadResult.UnavailableComponents.Add(new StandardComponentRef(componentCollection, standardComponentName)); // No implementation for known type
                        }
                    }
                }

                foreach (Component component in Document.Components)
                    component.ApplyConnections(Document);

                // Reset metadata
                Document.Metadata = new CircuitDocumentMetadata();
                Document.Metadata.Format = "XML (1.1)";
                Document.Metadata.Extended.Application = application;
                Document.Metadata.Extended.AppVersion = appVersion;

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

        /// <summary>
        /// Converts a component name from the old syntax to its 2.0+ equivalent.
        /// </summary>
        /// <param name="oldType">The component name to convert.</param>
        /// <param name="t">Optional type parameter for the component.</param>
        /// <param name="collection">The collection the component belongs to.</param>
        /// <param name="newType">The new component name.</param>
        private static void ConvertComponentName(string oldType, string t, out string collection, out string newType)
        {
            newType = null;
            collection = CDDX.ComponentCollections.Standard;

            if (oldType == "logicgate" && t == "0")
                newType = "and2";
            else if (oldType == "logicgate" && t == "1")
                newType = "nand2";
            else if (oldType == "logicgate" && t == "2")
                newType = "or2";
            else if (oldType == "logicgate" && t == "3")
                newType = "nor2";
            else if (oldType == "logicgate" && t == "4")
                newType = "xor2";
            else if (oldType == "logicgate" && t == "5")
                newType = "not";
            else if (oldType == "logicgate" && t == "6")
                newType = "schmittnot";
            else if (oldType == "supply")
                newType = "cell";
            else if (oldType == "switch" && t == "0")
                newType = "pushswitch";
            else if (oldType == "switch" && t == "1")
                newType = "toggleswitch";
            else if (oldType == "transistor" && t == "0")
                newType = "mosfetn";
            else if (oldType == "transistor" && t == "1")
                newType = "mosfetp";
            else if (oldType == "transistor" && t == "2")
                newType = "transnpn";
            else if (oldType == "transistor" && t == "3")
                newType = "transpnp";
            else if (oldType == "outputdevice" && t == "3")
                newType = "heater";
            else if (oldType == "mosfet")
                newType = "mosfetn";
            else if (oldType == "externalconnection")
            {
                newType = "extconnection";
                collection = CDDX.ComponentCollections.Misc;
            }
            else if (oldType == "counter")
                newType = "counter4";
            else if (oldType == "switch")
                newType = "pushswitch";
            else if (oldType == "microphone")
                newType = "microphone";
            else
            {
                newType = oldType;
                collection = null;
            }
        }

        /// <summary>
        /// Converts properties from the old syntax to their 2.0+ equivalent.
        /// </summary>
        /// <param name="componentCollection">Collection which the component belongs to.</param>
        /// <param name="standardComponentName">Component which the properties belong to.</param>
        /// <param name="properties">The properties to convert.</param>
        private void ConvertProperties(string componentCollection, string standardComponentName, Dictionary<string, object> properties)
        {
            if (componentCollection == CDDX.ComponentCollections.Misc && standardComponentName == "extconnection" && properties.ContainsKey("topleft"))
            {
                bool textpos = !bool.Parse(properties["topleft"].ToString());
                properties.Remove("topleft");
                properties.Add("textpos", textpos);
            }
        }
    }
}
