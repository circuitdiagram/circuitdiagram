#region Copyright & License Information
/*
 * Copyright 2012-2015 Sam Fisher
 *
 * This file is part of Circuit Diagram
 * http://www.circuit-diagram.org/
 * 
 * Circuit Diagram is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or (at
 * your option) any later version.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CircuitDiagram.Components.Description.Render;
using System.Windows.Media;
using CircuitDiagram.Components;
using CircuitDiagram.Render.Path;
using System.Text.RegularExpressions;
using CircuitDiagram.Render;
using CircuitDiagram.Components.Description;
using CircuitDiagram.Components.Conditions;
using CircuitDiagram.Components.Conditions.Parsers;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Loads component descriptions from an XML file.
    /// </summary>
    public class XmlLoader : IComponentDescriptionLoader
    {
        ComponentDescription[] m_descriptions;

        public ComponentDescription[] GetDescriptions()
        {
            return m_descriptions;
        }

        public bool Load(System.IO.Stream stream)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);

                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("cd", "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/component/xml");

                // Read format version
                Version formatVersion = new Version(1, 0);
                if (doc.DocumentElement.HasAttribute("version"))
                    formatVersion = new Version(doc.DocumentElement.Attributes["version"].InnerText);

                // Decide which condition parser to use
                IConditionParser conditionParser;
                if (formatVersion >= new Version(1, 1))
                    conditionParser = new ConditionParser();
                else
                    conditionParser = new LegacyConditionParser();

                string name = null;
                bool canResize = true;
                bool canFlip = false;
                double minSize = ComponentHelper.GridSize;
                string version = null;
                string author = null;
                string additionalInformation = null;
                string implementationSet = null;
                string implementationName = null;
                Guid guid = Guid.Empty;

                // Metadata
                XmlNodeList metaNodes = doc.SelectNodes("/cd:component/cd:declaration/cd:meta", namespaceManager);
                foreach (XmlNode metaNode in metaNodes)
                {
                    string metaName = metaNode.Attributes["name"].InnerText.ToLowerInvariant();
                    string metaValue = metaNode.Attributes["value"].InnerText;

                    if (metaName == "name")
                        name = metaValue;
                    else if (metaName == "canresize" && metaValue.ToLower() == "false")
                        canResize = false;
                    else if (metaName == "canflip" && metaValue.ToLower() == "true")
                        canFlip = true;
                    else if (metaName == "minsize")
                        minSize = double.Parse(metaValue);
                    else if (metaName == "version")
                        version = metaValue;
                    else if (metaName == "author")
                        author = metaValue;
                    else if (metaName == "additionalinformation")
                        additionalInformation = metaValue;
                    else if (metaName == "implementset")
                        implementationSet = metaValue;
                    else if (metaName == "implementitem")
                        implementationName = metaValue;
                    else if (metaName == "guid")
                        guid = new Guid(metaValue);
                }

                // Check if component is valid - return false if not
                if (String.IsNullOrEmpty(name))
                    return false;

                List<Conditional<FlagOptions>> flagOptions = new List<Conditional<FlagOptions>>();
                XmlNodeList flagNodes = doc.SelectNodes("/cd:component/cd:declaration/cd:flags", namespaceManager);
                foreach (XmlElement flagGroup in flagNodes)
                {
                    IConditionTreeItem conditions = ConditionTree.Empty;
                    if (flagGroup.HasAttribute("conditions"))
                    {
                        conditions = conditionParser.Parse(flagGroup.Attributes["conditions"].InnerText);
                    }

                    FlagOptions theOptions = FlagOptions.None;
                    foreach (XmlNode node in flagGroup.ChildNodes)
                        theOptions |= (FlagOptions)Enum.Parse(typeof(FlagOptions), node.InnerText, true);

                    flagOptions.Add(new Conditional<FlagOptions>(theOptions, conditions));
                }

                // Properties
                List<ComponentProperty> parsedComponentProperties = new List<ComponentProperty>();
                XmlNodeList propertyNodes = doc.SelectNodes("/cd:component/cd:declaration/cd:property", namespaceManager);
                foreach (XmlElement propertyElement in propertyNodes)
                {
                    string propertyName = propertyElement.Attributes["name"].InnerText;
                    string type = propertyElement.Attributes["type"].InnerText;
                    string defaultValue = propertyElement.Attributes["default"].InnerText;
                    string serializeAs = propertyElement.Attributes["serialize"].InnerText;
                    string display = propertyElement.Attributes["display"].InnerText;

                    Type propertyType;
                    switch (type.ToLowerInvariant())
                    {
                        case "double":
                            propertyType = typeof(double);
                            break;
                        case "int":
                            propertyType = typeof(int);
                            break;
                        case "bool":
                            propertyType = typeof(bool);
                            break;
                        default:
                            propertyType = typeof(string);
                            break;
                    }

                    object propertyDefaultValue = defaultValue;
                    if (propertyType == typeof(double))
                        propertyDefaultValue = double.Parse(defaultValue);
                    else if (propertyType == typeof(int))
                        propertyDefaultValue = int.Parse(defaultValue);
                    else if (propertyType == typeof(bool))
                        propertyDefaultValue = bool.Parse(defaultValue);

                    List<string> propertyOptions = null;
                    if (type == "enum")
                    {
                        propertyOptions = new List<string>();
                        XmlNodeList optionNodes = propertyElement.SelectNodes("cd:option", namespaceManager);
                        foreach (XmlNode optionNode in optionNodes)
                            propertyOptions.Add(optionNode.InnerText);
                    }

                    List<ComponentPropertyFormat> formatRules = new List<ComponentPropertyFormat>();
                    if (((XmlElement)propertyElement).HasAttribute("format"))
                        formatRules.Add(new ComponentPropertyFormat(propertyElement["format"].InnerText, ConditionTree.Empty));
                    else
                    {
                        XmlNodeList formatRuleNodes = propertyElement.SelectNodes("cd:formatting/cd:format", namespaceManager);
                        foreach (XmlElement formatNode in formatRuleNodes)
                        {
                            IConditionTreeItem conditionCollection = ConditionTree.Empty;
                            if (formatNode.HasAttribute("conditions"))
                            {
                                conditionCollection = conditionParser.Parse(formatNode.Attributes["conditions"].InnerText);
                            }

                            formatRules.Add(new ComponentPropertyFormat(formatNode.Attributes["value"].InnerText, conditionCollection));
                        }
                    }

                    Dictionary<PropertyOtherConditionType, IConditionTreeItem> otherConditions = new Dictionary<PropertyOtherConditionType, IConditionTreeItem>();
                    XmlNodeList otherConditionsNodes = propertyElement.SelectNodes("cd:other/cd:conditions", namespaceManager);
                    foreach (XmlNode otherConditionNode in otherConditionsNodes)
                    {
                        XmlElement element = otherConditionNode as XmlElement;
                        if (element != null && element.HasAttribute("for") && element.HasAttribute("value"))
                        {
                            string conditionsFor = element.Attributes["for"].InnerText;
                            string conditionsString = element.Attributes["value"].InnerText;
                            IConditionTreeItem conditionCollection;
                            conditionCollection = conditionParser.Parse(conditionsString);

                            if (Enum.IsDefined(typeof(PropertyOtherConditionType), conditionsFor))
                                otherConditions.Add((PropertyOtherConditionType)Enum.Parse(typeof(PropertyOtherConditionType), conditionsFor, true), conditionCollection);
                        }
                    }

                    ComponentProperty property = new ComponentProperty(propertyName, serializeAs, display, propertyType, propertyDefaultValue, formatRules.ToArray(), otherConditions, (propertyOptions == null ? null : propertyOptions.ToArray()));
                    parsedComponentProperties.Add(property);
                }

                // Configurations
                XmlNodeList configurationNodes = doc.SelectNodes("/cd:component/cd:declaration/cd:configurations/cd:configuration", namespaceManager);
                List<ComponentConfiguration> componentConfigurations = new List<ComponentConfiguration>(configurationNodes.Count);
                foreach (XmlNode node in configurationNodes)
                {
                    string configName = node.Attributes["name"].InnerText;
                    string configValue = node.Attributes["value"].InnerText;
                    string configImplements = null;
                    if ((node as XmlElement).HasAttribute("implements"))
                        configImplements = node.Attributes["implements"].InnerText;

                    ComponentConfiguration newConfiguration = new ComponentConfiguration(configImplements, configName, new Dictionary<string,object>());

                    string[] setters = configValue.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string setter in setters)
                    {
                        string[] tempSplit = setter.Split(':');
                        string propertyName = tempSplit[0];
                        string value = tempSplit[1];

                        // convert from string to proper type
                        foreach (ComponentProperty property in parsedComponentProperties)
                        {
                            if (propertyName == property.Name)
                            {
                                object setterValue = value;
                                if (property.Type == typeof(double))
                                    setterValue = double.Parse(value);
                                else if (property.Type == typeof(int))
                                    setterValue = int.Parse(value);
                                else if (property.Type == typeof(bool))
                                    setterValue = bool.Parse(value);

                                newConfiguration.Setters.Add(property.SerializedName, setterValue);

                                break;
                            }
                        }
                    }

                    componentConfigurations.Add(newConfiguration);
                }

                // Connections
                List<ConnectionGroup> parsedConnectionGroups = new List<ConnectionGroup>();
                XmlNodeList connectionGroupNodes = doc.SelectNodes("/cd:component/cd:connections/cd:group", namespaceManager);
                foreach (XmlNode connectionGroupNode in connectionGroupNodes)
                {
                    IConditionTreeItem conditionCollection = ConditionTree.Empty;
                    List<ConnectionDescription> connections = new List<ConnectionDescription>();

                    if ((connectionGroupNode as XmlElement).HasAttribute("conditions"))
                    {
                        conditionCollection = conditionParser.Parse(connectionGroupNode.Attributes["conditions"].InnerText);
                    }

                    foreach (XmlNode connectionNode in connectionGroupNode.ChildNodes)
                    {
                        if (connectionNode.Name.ToLowerInvariant() == "connection")
                        {
                            ConnectionEdge edge = ConnectionEdge.None;
                            if ((connectionNode as XmlElement).HasAttribute("edge"))
                            {
                                string edgeText = connectionNode.Attributes["edge"].InnerText.ToLowerInvariant();
                                if (edgeText == "start")
                                    edge = ConnectionEdge.Start;
                                else if (edgeText == "end")
                                    edge = ConnectionEdge.End;
                                else if (edgeText == "both")
                                    edge = ConnectionEdge.Both;
                            }
                            string connectionName = "#";
                            if ((connectionNode as XmlElement).HasAttribute("name"))
                                connectionName = connectionNode.Attributes["name"].InnerText;
                            connections.Add(new ConnectionDescription(new ComponentPoint(connectionNode.Attributes["start"].InnerText), new ComponentPoint(connectionNode.Attributes["end"].InnerText), edge, connectionName));
                        }
                    }

                    parsedConnectionGroups.Add(new ConnectionGroup(conditionCollection, connections.ToArray()));
                }

                // Render descriptions
                List<RenderDescription> parsedRenderDescriptions = new List<RenderDescription>();
                XmlNodeList renderDescriptions = doc.SelectNodes("/cd:component/cd:render/cd:group", namespaceManager);
                foreach (XmlNode renderNode in renderDescriptions)
                {
                    IConditionTreeItem conditionCollection = ConditionTree.Empty;
                    List<IRenderCommand> commands = new List<IRenderCommand>();

                    if ((renderNode as XmlElement).HasAttribute("conditions"))
                    {
                        conditionCollection = conditionParser.Parse(renderNode.Attributes["conditions"].InnerText);
                    }

                    foreach (XmlNode renderCommandNode in renderNode.ChildNodes)
                    {
                        if (renderCommandNode.Name.ToLower() == "line")
                        {
                            double thickness = 2d;
                            if ((renderCommandNode as XmlElement).HasAttribute("thickness"))
                                thickness = double.Parse(renderCommandNode.Attributes["thickness"].InnerText);
                            Line line = new Line(new ComponentPoint(renderCommandNode.Attributes["start"].InnerText), new ComponentPoint(renderCommandNode.Attributes["end"].InnerText), thickness);
                            commands.Add(line);
                        }
                        else if (renderCommandNode.Name.ToLowerInvariant() == "rect")
                        {
                            double thickness = 2d;
                            bool fill = false;
                            ComponentPoint location;
                            if ((renderCommandNode as XmlElement).HasAttribute("location"))
                                location = new ComponentPoint(renderCommandNode.Attributes["location"].InnerText);
                            else
                            {
                                string x = renderCommandNode.Attributes["x"].InnerText;
                                string y = renderCommandNode.Attributes["y"].InnerText;
                                location = new ComponentPoint(x, y);
                            }
                            if ((renderCommandNode as XmlElement).HasAttribute("fill") && renderCommandNode.Attributes["fill"].InnerText.ToLowerInvariant() == "true")
                                fill = true;
                            double width = double.Parse(renderCommandNode.Attributes["width"].InnerText);
                            double height = double.Parse(renderCommandNode.Attributes["height"].InnerText);
                            Rectangle rectangle = new Rectangle(location, width, height, thickness, fill);
                            commands.Add(rectangle);
                        }
                        else if (renderCommandNode.Name.ToLowerInvariant() == "ellipse")
                        {
                            double thickness = 2d;
                            if ((renderCommandNode as XmlElement).HasAttribute("thickness"))
                                thickness = double.Parse(renderCommandNode.Attributes["thickness"].InnerText);
                            bool fill = false;
                            if ((renderCommandNode as XmlElement).HasAttribute("fill") && renderCommandNode.Attributes["fill"].InnerText.ToLowerInvariant() == "true")
                                fill = true;
                            ComponentPoint centre;
                            if ((renderCommandNode as XmlElement).HasAttribute("centre"))
                                centre = new ComponentPoint(renderCommandNode.Attributes["centre"].InnerText);
                            else
                            {
                                string x = renderCommandNode.Attributes["x"].InnerText;
                                string y = renderCommandNode.Attributes["y"].InnerText;
                                centre = new ComponentPoint(x, y);
                            }
                            double radiusX = double.Parse(renderCommandNode.Attributes["radiusx"].InnerText);
                            double radiusY = double.Parse(renderCommandNode.Attributes["radiusy"].InnerText);
                            Ellipse ellipse = new Ellipse(centre, radiusX, radiusY, thickness, fill);
                            commands.Add(ellipse);
                        }
                        else if (renderCommandNode.Name.ToLowerInvariant() == "text")
                        {
                            ComponentPoint location;
                            if ((renderCommandNode as XmlElement).HasAttribute("location"))
                                location = new ComponentPoint(renderCommandNode.Attributes["location"].InnerText);
                            else
                            {
                                string x = renderCommandNode.Attributes["x"].InnerText;
                                string y = renderCommandNode.Attributes["y"].InnerText;
                                location = new ComponentPoint(x, y);
                            }

                            string tAlignment = "TopLeft";
                            if ((renderCommandNode as XmlElement).HasAttribute("align"))
                                tAlignment = renderCommandNode.Attributes["align"].InnerText;
                            TextAlignment alignment = (TextAlignment)Enum.Parse(typeof(TextAlignment), tAlignment, true);

                            double size = ComponentHelper.SmallTextSize;
                            if ((renderCommandNode as XmlElement).HasAttribute("size"))
                            {
                                if (renderCommandNode.Attributes["size"].InnerText.ToLowerInvariant() == "large")
                                    size = ComponentHelper.LargeTextSize;
                            }

                            List<TextRun> textRuns = new List<TextRun>();                                
                            XmlNode textValueNode = renderCommandNode.SelectSingleNode("cd:value", namespaceManager);
                            if (textValueNode != null)
                            {
                                foreach (XmlNode spanNode in textValueNode.ChildNodes)
                                {
                                    string nodeValue = spanNode.InnerText;

                                    if (spanNode.Name.ToLowerInvariant() == "span")
                                        textRuns.Add(new TextRun(nodeValue, TextRunFormatting.Normal));
                                    else if (spanNode.Name.ToLowerInvariant() == "sub")
                                        textRuns.Add(new TextRun(nodeValue, TextRunFormatting.Subscript));
                                    else if (spanNode.Name.ToLowerInvariant() == "sup")
                                        textRuns.Add(new TextRun(nodeValue, TextRunFormatting.Superscript));
                                }
                            }
                            else
                            {
                                textRuns.Add(new TextRun(renderCommandNode.Attributes["value"].InnerText, new TextRunFormatting(TextRunFormattingType.Normal, size)));
                            }

                            Text text = new Text(location, alignment, textRuns);
                            commands.Add(text);
                        }
                        else if (renderCommandNode.Name.ToLowerInvariant() == "path")
                        {
                            double thickness = 2d;
                            if ((renderCommandNode as XmlElement).HasAttribute("thickness"))
                                thickness = double.Parse(renderCommandNode.Attributes["thickness"].InnerText);

                            string location = null;
                            if ((renderCommandNode as XmlElement).HasAttribute("start"))
                                location = renderCommandNode.Attributes["start"].InnerText;

                            bool fill = false;
                            if ((renderCommandNode as XmlElement).HasAttribute("fill") && renderCommandNode.Attributes["fill"].InnerText.ToLowerInvariant() == "true")
                                fill = true;

                            string data = renderCommandNode.Attributes["data"].InnerText;

                            commands.Add(RenderPath.Parse(location, data, thickness, fill));
                        }
                    }

                    parsedRenderDescriptions.Add(new RenderDescription(conditionCollection, commands.ToArray()));
                }
                
                m_descriptions = new ComponentDescription[1];
                ComponentDescriptionMetadata metadata = new ComponentDescriptionMetadata();
                metadata.Type = String.Format("XML {0} (*.xml)", formatVersion.ToString(2));
                if (version != null)
                    metadata.Version = new Version(version);
                metadata.Configurations.AddRange(componentConfigurations);
                metadata.Author = author;
                metadata.AdditionalInformation = additionalInformation;
                metadata.ImplementSet = implementationSet;
                metadata.ImplementItem = implementationName;
                metadata.GUID = guid;
                m_descriptions[0] = new ComponentDescription("#", name, canResize, canFlip, minSize, parsedComponentProperties.ToArray(), parsedConnectionGroups.ToArray(), parsedRenderDescriptions.ToArray(), flagOptions.ToArray(), metadata);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}