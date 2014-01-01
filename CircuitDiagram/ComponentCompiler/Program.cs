// Program.cs
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
using NDesk.Options;
using System.Xml;
using System.IO;
using CircuitDiagram.Components;
using CircuitDiagram.IO;
using CircuitDiagram.Components.Description;

namespace cdcompile
{
    class Program
    {
        static void Main(string[] args)
        {
            string output = null;
            string config = null;
            bool help = false;
            var p = new OptionSet() {
                { "o|output=", v => output = v },
                { "i|input|config=", v => config = v },
   	            { "h|?|help",   v => help = v != null },
            };
            List<string> extra = p.Parse(args);

            List<ComponentDescription> componentDescriptions = new List<ComponentDescription>();
            List<BinaryResource> binaryResources = new List<BinaryResource>();

            string keyPath = null;
            if (config != null && File.Exists(config))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(config);

                XmlElement root = doc.SelectSingleNode("/cdcom") as XmlElement;
                string markupVersion = (root.HasAttribute("markupversion") ? root.Attributes["markupversion"].InnerText : null);
                string xUiApp = (root.HasAttribute("x-uiapp") ? root.Attributes["x-uiapp"].InnerText : null);
                keyPath = (root.HasAttribute("key") ? root.Attributes["key"].InnerText : null);

                XmlNodeList componentNodes = doc.SelectNodes("/cdcom/component");
                foreach (XmlNode component in componentNodes)
                {
                    string internalId = "C0";
                    if ((component as XmlElement).HasAttribute("id"))
                        internalId = component.Attributes["id"].InnerText;
                    string path = component.Attributes["path"].InnerText;

                    XmlLoader loader = new CircuitDiagram.IO.XmlLoader();
                    loader.Load(new FileStream(path, FileMode.Open));

                    ComponentDescription description = loader.GetDescriptions()[0];
                    description.ID = internalId;

                    foreach (XmlNode iconNode in component.SelectNodes("icon"))
                    {
                        if ((iconNode as XmlElement).HasAttribute("configuration"))
                        {
                            ComponentConfiguration matchedConfiguration = description.Metadata.Configurations.FirstOrDefault(configuration => configuration.Name == iconNode.Attributes["configuration"].InnerText);
                            if (matchedConfiguration != null)
                            {
                                byte[] iconData = File.ReadAllBytes(iconNode.InnerText);
                                matchedConfiguration.IconData = iconData;
                                matchedConfiguration.IconMimeType = "image/png";
                            }
                        }
                        else
                        {
                            byte[] iconData = File.ReadAllBytes(iconNode.InnerText);
                            description.Metadata.IconData = iconData;
                            description.Metadata.IconMimeType = "image/png";
                        }
                    }

                    componentDescriptions.Add(description);
                }

                #region old
                /*XmlNodeList resourceNodes = doc.SelectNodes("/cdcom/resource");
                foreach (XmlNode resourceNode in resourceNodes)
                {
                    string resourceId = resourceNode.Attributes["id"].InnerText;
                    string type = resourceNode.Attributes["type"].InnerText;
                    XmlElement resourceElement = resourceNode as XmlElement;
                    string target = (resourceElement.HasAttribute("target") ? resourceElement.Attributes["target"].InnerText : null);
                    string componentId = (resourceElement.HasAttribute("componentid") ? resourceElement.Attributes["componentid"].InnerText : null);
                    string path = (resourceElement.HasAttribute("path") ? resourceElement.Attributes["path"].InnerText : null);
                    string use = (resourceElement.HasAttribute("target") ? resourceElement.Attributes["target"].InnerText : null);
                    BinaryResourceUse rUse = BinaryResourceUse.None;
                    if (use.ToLowerInvariant() == "icon")
                        rUse = BinaryResourceUse.Icon;
                    else if (use.ToLowerInvariant() == "metadata")
                        rUse = BinaryResourceUse.Metadata;
                    else if (use.ToLowerInvariant() == "configurations")
                        rUse = BinaryResourceUse.Configurations;

                    if (type == "application/x-metadata" && path == null)
                    {
                        string author = resourceNode.SelectSingleNode("author").InnerText;
                        string version = resourceNode.SelectSingleNode("version").InnerText;
                        string additionalInformation = resourceNode.SelectSingleNode("additionalinformation").InnerText;
                        string guid = resourceNode.SelectSingleNode("guid").InnerText;

                        MemoryStream metadataStream = new MemoryStream();
                        System.IO.BinaryWriter metadataWriter = new System.IO.BinaryWriter(metadataStream);
                        metadataWriter.Write("Circuit Diagram");
                        metadataWriter.Write("http://www.circuit-diagram.org/");
                        if (String.IsNullOrEmpty(guid))
                            metadataWriter.Write(Guid.NewGuid().ToByteArray());
                        else
                            metadataWriter.Write(new Guid(guid).ToByteArray());
                        metadataWriter.Write(version);

                        binaryResources.Add(new BinaryResource(resourceId, componentId, type, rUse, metadataStream.ToArray()));
                    }
                    else if (rUse == BinaryResourceUse.Icon)
                    {
                        MemoryStream tempStream = new MemoryStream();
                        System.IO.BinaryWriter tempWriter = new System.IO.BinaryWriter(tempStream);

                        byte[] data = System.IO.File.ReadAllBytes(path);
                        if (Path.GetExtension(path) != ".svg")
                            tempWriter.Write("image/png");
                        else
                            tempWriter.Write("image/svg+xml");
                        tempWriter.Write(data.Length);
                        tempWriter.Write(data);
                        
                        int numAdditional = 0;
                        foreach (XmlElement node in resourceNode.ChildNodes)
                            if (node.Name == "icon" && node.HasAttribute("path"))
                                numAdditional++;
                        tempWriter.Write(numAdditional);

                        foreach (XmlElement node in resourceNode.ChildNodes)
                        {
                            if (node.Name == "icon" && node.HasAttribute("path"))
                            {
                                byte[] data2 = System.IO.File.ReadAllBytes(node.Attributes["path"].InnerText);
                                ComponentDescriptionConditionCollection conditions = new ComponentDescriptionConditionCollection();
                                if (node.HasAttribute("conditions"))
                                    conditions = ComponentDescriptionConditionCollection.Parse(node.Attributes["conditions"].InnerText);
                                CircuitDiagram.IO.BinaryIOExtentions.Write(tempWriter, conditions);
                                if (Path.GetExtension(node.Attributes["path"].InnerText) != ".svg")
                                    tempWriter.Write("image/png");
                                else
                                    tempWriter.Write("image/svg+xml");
                                tempWriter.Write(data2.Length);
                                tempWriter.Write(data2);
                            }
                        }

                        binaryResources.Add(new BinaryResource(resourceId, componentId, type, rUse, tempStream.ToArray()));
                    }
                    else if (rUse == BinaryResourceUse.Configurations)
                    {
                        MemoryStream tempStream = new MemoryStream();
                        System.IO.BinaryWriter tempWriter = new System.IO.BinaryWriter(tempStream);

                        int numConfigurations = 0;
                        foreach (XmlElement node in resourceNode.ChildNodes)
                            if (node.Name == "configuration" && node.HasAttribute("name"))
                                numConfigurations++;
                        tempWriter.Write(numConfigurations);

                        foreach (XmlElement node in resourceNode.ChildNodes)
                        {
                            if (node.Name == "configuration" && node.HasAttribute("name"))
                            {
                                tempWriter.Write(node.Attributes["name"].InnerText);
                                if (node.HasAttribute("value"))
                                {
                                    string settersString = node.Attributes["value"].InnerText;
                                    string[] setters = settersString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    tempWriter.Write(setters.Length); // number of setters
                                    foreach (string setter in setters)
                                    {
                                        string[] tempSplit = setter.Split(':');
                                        string propertyName = tempSplit[0];
                                        string value = tempSplit[1];
                                        
                                        // convert from string to proper type
                                        ComponentDescription component = null;
                                        foreach (ComponentDescription description in componentDescriptions)
                                            if (description.ID == componentId)
                                                component = description;
                                        if (component != null)
                                        {
                                            foreach (ComponentProperty property in component.Properties)
                                            {
                                                if (property.SerializedName == propertyName || propertyName == "$" + property.Name)
                                                {
                                                    object setterValue = value;
                                                    if (property.Type == typeof(double))
                                                        setterValue = double.Parse(value);
                                                    else if (property.Type == typeof(int))
                                                        setterValue = int.Parse(value);
                                                    else if (property.Type == typeof(bool))
                                                        setterValue = bool.Parse(value);

                                                    // write
                                                    tempWriter.Write(property.SerializedName);
                                                    tempWriter.WriteType(setterValue, property.EnumOptions != null);

                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        binaryResources.Add(new BinaryResource(resourceId, componentId, type, rUse, tempStream.ToArray()));
                    }
                    else
                    {
                        binaryResources.Add(new BinaryResource(resourceId, componentId, type, rUse, System.IO.File.ReadAllBytes(path)));
                    }
                }*/
                #endregion
            }

            FileStream stream = new FileStream(output, FileMode.Create, FileAccess.Write);

            string keyString = null;
            if (keyPath != null && File.Exists(keyPath))
                keyString = File.ReadAllText(keyPath);
            System.Security.Cryptography.RSAParameters? key = null;
            if (keyString != null)
            {
                System.Security.Cryptography.RSACryptoServiceProvider RSAalg = new System.Security.Cryptography.RSACryptoServiceProvider();
                RSAalg.FromXmlString(keyString);
                key = RSAalg.ExportParameters(true);
            }

            CircuitDiagram.IO.BinaryWriter.BinaryWriterSettings settings = new CircuitDiagram.IO.BinaryWriter.BinaryWriterSettings();
            settings.Key = key;
            CircuitDiagram.IO.BinaryWriter writer = new CircuitDiagram.IO.BinaryWriter(stream, settings);
            writer.Descriptions.AddRange(componentDescriptions);
            writer.Resources.AddRange(binaryResources);
            writer.Write();
            stream.Flush();
        }
    }
}
