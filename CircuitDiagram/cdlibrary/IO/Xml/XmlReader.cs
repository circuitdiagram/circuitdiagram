// XmlReader.cs
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

namespace CircuitDiagram.IO.Xml
{
    /// <summary>
    /// Reads a circuit document in the Xml format from a stream.
    /// </summary>
    public class XmlReader : IDocumentReader
    {
        public string PluginPartName { get { return "Xml Reader"; } }

        public string FileTypeName { get { return "XML Files"; } }

        public string FileTypeExtension { get { return ".xml"; } }

        public Guid GUID { get { return new Guid("f90e8458-3615-4f8a-9ba6-4115c6f2d6ba"); } }

        public IODocument Document { get; private set; }

        public DocumentLoadResult LoadResult { get; private set; }

        public bool Load(System.IO.Stream stream)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);

                double formatVersion = 1.0d;
                if ((doc.SelectSingleNode("/circuit") as XmlElement).HasAttribute("version"))
                {
                    string versionString = doc.SelectSingleNode("/circuit").Attributes["version"].InnerText;
                    double.TryParse(versionString, out formatVersion);
                }

                if (formatVersion == 1.1d)
                    return LoadVersion1_1(doc);
                else if (formatVersion == 1.0d)
                    return LoadVersion1_0(doc);

                DocumentLoadResult loadResult = new DocumentLoadResult();
                loadResult.Type = DocumentLoadResultType.FailNewerVersion;
                return false;
            }
            catch (Exception)
            {
                LoadResult = new DocumentLoadResult();
                LoadResult.Type = DocumentLoadResultType.FailIncorrectFormat;
                return false;
            }
        }

        public bool IsDescriptionEmbedded(IOComponentType type)
        {
            // Xml documents cannot have embedded descriptions.
            return false;
        }

        public EmbedComponentData GetEmbeddedDescription(IOComponentType type)
        {
            // No descriptions are embedded.
            return null;
        }

        public void Dispose()
        {
            // Do nothing
        }

        private bool LoadVersion1_0(XmlDocument doc)
        {
            try
            {
                DocumentLoadResult loadResult = new DocumentLoadResult();

                Document = new IODocument();

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
                    double x = Math.Round(double.Parse(locationSplit[0]) / 10d) * 10d;
                    double y = Math.Round(double.Parse(locationSplit[1]) / 10d) * 10d;
                    double endX = Math.Round(double.Parse(locationSplit[2]) / 10d) * 10d;
                    double endY = Math.Round(double.Parse(locationSplit[3]) / 10d) * 10d;
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

                    if (lType == "wire")
                    {
                        // Wire

                        IOWire wire = new IOWire();
                        wire.Location = new System.Windows.Point(x, y);
                        wire.Size = size;
                        wire.Orientation = (horizontal ? Orientation.Horizontal : Orientation.Vertical);

                        // Add to document
                        Document.Wires.Add(wire);
                    }
                    else
                    {
                        // Full component

                        IOComponent component = new IOComponent();
                        component.Location = new System.Windows.Point(x, y);
                        component.Size = size;
                        component.Orientation = (horizontal ? Orientation.Horizontal : Orientation.Vertical);

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

                        // Set properties
                        ConvertProperties(componentCollection, standardComponentName, properties);
                        foreach (var property in properties)
                            component.Properties.Add(new IOComponentProperty(property.Key, property.Value, true));

                        // Set type
                        component.Type = new IOComponentType(componentCollection, standardComponentName);
                        component.Type.Name = type;

                        // Add to document
                        Document.Components.Add(component);
                    }
                }

                // Reset metadata
                Document.Metadata = new CircuitDocumentMetadata();
                LoadResult.Format = "XML (1.0)";
                Document.Metadata.Application = application;
                Document.Metadata.AppVersion = appVersion;

                LoadResult.Type = DocumentLoadResultType.Success;
                return true;
            }
            catch (Exception)
            {
                // Wrong format
                LoadResult = new DocumentLoadResult();
                LoadResult.Type = DocumentLoadResultType.FailIncorrectFormat;
                return false;
            }
        }

        private bool LoadVersion1_1(XmlDocument doc)
        {
            try
            {
                Document = new IODocument();

                XmlElement circuitNode = doc.SelectSingleNode("/circuit") as XmlElement;
                if (circuitNode.HasAttribute("width") && circuitNode.HasAttribute("height"))
                    Document.Size = new System.Windows.Size(double.Parse(circuitNode.Attributes["width"].InnerText), double.Parse(circuitNode.Attributes["height"].InnerText));
                string application = null;
                string appVersion = null;
                if (circuitNode.HasAttribute("application"))
                {
                    application = circuitNode.Attributes["application"].InnerText;
                    if (circuitNode.HasAttribute("appversion"))
                        appVersion = circuitNode.Attributes["appversion"].InnerText;
                }
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
                    double size = 10d;
                    if ((node as XmlElement).HasAttribute("size"))
                        size = double.Parse(node.Attributes["size"].InnerText);
                    bool horizontal = true;
                    if ((node as XmlElement).HasAttribute("orientation") && node.Attributes["orientation"].InnerText.ToLowerInvariant() == "vertical")
                        horizontal = false;
                    Guid guid = Guid.Empty;
                    if ((node as XmlElement).HasAttribute("guid"))
                        guid = new Guid(node.Attributes["guid"].InnerText);

                    // Check component type
                    string lType = type.ToLowerInvariant();

                    if (lType == "wire")
                    {
                        // Element is wire

                        IOWire wire = new IOWire();
                        wire.Location = new System.Windows.Point(x, y);
                        wire.Orientation = (horizontal ? Orientation.Horizontal : Orientation.Vertical);
                        wire.Size = size;

                        // Add to document
                        Document.Wires.Add(wire);
                    }
                    else
                    {
                        // Element is component

                        IOComponent component = new IOComponent();
                        component.Location = new System.Windows.Point(x, y);
                        component.Orientation = (horizontal ? Orientation.Horizontal : Orientation.Vertical);
                        component.Size = size;

                        // Other properties
                        Dictionary<string, object> properties = new Dictionary<string, object>(node.Attributes.Count);
                        foreach (XmlAttribute attribute in node.Attributes)
                            properties.Add(attribute.Name, attribute.InnerText);

                        // Read type parameter
                        string t = null;
                        if ((node as XmlElement).HasAttribute("t"))
                            t = node.Attributes["t"].InnerText;
                        if (t == null && lType == "logicgate" && (node as XmlElement).HasAttribute("logictype"))
                            t = node.Attributes["logictype"].InnerText;

                        if (properties.ContainsKey("flipped"))
                        {
                            component.IsFlipped = bool.Parse(properties["flipped"].ToString());
                            properties.Remove("flipped");
                        }

                        // Convert component name
                        string componentCollection;
                        string standardComponentName;
                        ConvertComponentName(lType, t, out componentCollection, out standardComponentName);

                        // Set properties
                        ConvertProperties(componentCollection, standardComponentName, properties);
                        foreach (var property in properties)
                            component.Properties.Add(new IOComponentProperty(property.Key, property.Value, true));

                        // Set type
                        component.Type = new IOComponentType(componentCollection, standardComponentName);
                        component.Type.Name = type;
                        component.Type.GUID = guid;

                        // Add to document
                        Document.Components.Add(component);
                    }
                }

                // Set metadata
                Document.Metadata.Application = application;
                Document.Metadata.AppVersion = appVersion;

                LoadResult = new DocumentLoadResult();
                LoadResult.Format = "XML (1.1)";
                LoadResult.Type = DocumentLoadResultType.Success;

                return true;
            }
            catch (Exception)
            {
                // Wrong format
                LoadResult = new DocumentLoadResult();
                LoadResult.Type = DocumentLoadResultType.FailIncorrectFormat;
                return false;
            }
        }

        private void RemoveStandardProperties(Dictionary<string, object> properties)
        {
            string[] keysToRemove = new string[]
            {
                "x",
                "y",
                "size",
                "horizontal",
                "orientation",
                "type"
            };
            foreach (string key in keysToRemove)
                if (properties.ContainsKey(key))
                    properties.Remove(key);
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
            collection = ComponentCollections.Common;

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
            else if (oldType == "resistor" && (t == "0" || t == "5" || t == "Standard"))
                newType = "resistor";
            else if (oldType == "resistor" && (t == "1" || t == "Potentiometer"))
                newType = "potentiometer";
            else if (oldType == "resistor" && (t == "2" || t == "LDR"))
                newType = "ldr";
            else if (oldType == "resistor" && (t == "3" || t == "Thermistor"))
                newType = "thermistor";
            else if (oldType == "resistor" && (t == "4" || t == "Variable"))
                newType = "variableresistor";
            else if (oldType == "capacitor" & (t == "1" || t == "Variable"))
                newType = "variablecapacitor";
            else if (oldType == "capacitor" & (t == "2" || t == "Trimmer"))
                newType = "trimmercapacitor";
            else if (oldType == "capacitor" & (t == "3" || t == "Polarised"))
                newType = "polarisedcapacitor";
            else if (oldType == "capacitor")
                newType = "capacitor";
            else if (oldType == "rail")
                newType = "rail";
            else if (oldType == "supply")
                newType = "cell";
            else if (oldType == "ground")
                newType = "ground";
            else if (oldType == "switch" && t == "0")
                newType = "pushswitch";
            else if (oldType == "switch" && t == "1")
                newType = "toggleswitch";
            else if (oldType == "switch" && t == "2")
                newType = "analogueswitch";
            else if (oldType == "switch" && t == "3")
                newType = "ptbswitch";
            else if (oldType == "switch" && t == "4")
                newType = "changeoverswitch";
            else if (oldType == "transistor" && t == "0")
                newType = "transnpn";
            else if (oldType == "transistor" && t == "1")
                newType = "transpnp";
            else if (oldType == "transistor" && t == "2")
                newType = "mosfetn";
            else if (oldType == "transistor" && t == "3")
                newType = "mosfetp";
            else if (oldType == "mosfet")
                newType = "mosfetn";
            else if (oldType == "opamp")
                newType = "opamp";
            else if (oldType == "diode" && t == "0")
                newType = "diode";
            else if (oldType == "diode" && t == "1")
                newType = "zenerdiode";
            else if (oldType == "diode" && t == "2")
                newType = "led";
            else if (oldType == "diode" && t == "3")
                newType = "photodiode";
            else if (oldType == "diode" && t == "4")
                newType = "diodebridge";
            else if (oldType == "counter")
                newType = "counter4";
            else if (oldType == "switch")
                newType = "pushswitch";
            else if (oldType == "microphone")
                newType = "microphone";
            else if (oldType == "lamp")
                newType = "lamp";
            else if (oldType == "meter" && t == "0")
                newType = "voltmeter";
            else if (oldType == "meter" && t == "1")
                newType = "ammeter";
            else if (oldType == "inductor")
                newType = "inductor";
            else if (oldType == "transformer")
                newType = "transformer";
            else if (oldType == "crystal")
                newType = "crystal";
            else if (oldType == "segdisplay")
                newType = "7segdisplay";
            else if (oldType == "segdecoder")
                newType = "7segdecoder";
            else if (oldType == "microcontroller")
                newType = "microcontroller";
            else if (oldType == "outputdevice" && t == "0")
                newType = "speaker";
            else if (oldType == "outputdevice" && t == "1")
                newType = "motor";
            else if (oldType == "outputdevice" && t == "2")
                newType = "buzzer";
            else if (oldType == "outputdevice" && t == "3")
                newType = "heater";
            else if (oldType == "fuse")
                newType = "fuse";
            else if (oldType == "flipflop")
                newType = "dflipflop";
            else if (oldType == "externalconnection")
            {
                newType = "extconnection";
                collection = ComponentCollections.Misc;
            }
            else if (oldType == "elabel")
            {
                newType = "label";
                collection = ComponentCollections.Misc;
            }
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
            if (componentCollection == ComponentCollections.Misc && standardComponentName == "extconnection" && properties.ContainsKey("topleft"))
            {
                bool textpos = !bool.Parse(properties["topleft"].ToString());
                properties.Remove("topleft");
                properties.Add("textpos", textpos);
            }
        }
    }
}
