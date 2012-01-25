// XmlLoader.cs
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
using CircuitDiagram.Components.Render;
using System.Windows.Media;
using CircuitDiagram.Components;
using CircuitDiagram.Components.Render.Path;
using System.Text.RegularExpressions;

namespace CircuitDiagram.IO
{
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

                string name = null;
                bool canResize = true;
                bool canFlip = false;
                double minSize = ComponentHelper.GridSize;
                string version = null;
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
                    ComponentDescriptionConditionCollection conditions = new ComponentDescriptionConditionCollection();
                    if (flagGroup.HasAttribute("conditions"))
                    {
                        string[] conditionsString = flagGroup.Attributes["conditions"].InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string condition in conditionsString)
                            conditions.Add(ComponentDescriptionCondition.Parse(condition));
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
                        formatRules.Add(new ComponentPropertyFormat(propertyElement["format"].InnerText, new ComponentDescriptionConditionCollection()));
                    else
                    {
                        XmlNodeList formatRuleNodes = propertyElement.SelectNodes("cd:formatting/cd:format", namespaceManager);
                        foreach (XmlElement formatNode in formatRuleNodes)
                        {
                            ComponentDescriptionConditionCollection conditionCollection = new ComponentDescriptionConditionCollection();
                            if (formatNode.HasAttribute("conditions"))
                            {
                                string[] conditions = formatNode.Attributes["conditions"].InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string condition in conditions)
                                    conditionCollection.Add(ComponentDescriptionCondition.Parse(condition));
                            }

                            formatRules.Add(new ComponentPropertyFormat(formatNode.Attributes["value"].InnerText, conditionCollection));
                        }
                    }

                    Dictionary<PropertyOtherConditionType, ComponentDescriptionConditionCollection> otherConditions = new Dictionary<PropertyOtherConditionType, ComponentDescriptionConditionCollection>();
                    XmlNodeList otherConditionsNodes = propertyElement.SelectNodes("cd:other/cd:conditions", namespaceManager);
                    foreach (XmlNode otherConditionNode in otherConditionsNodes)
                    {
                        XmlElement element = otherConditionNode as XmlElement;
                        if (element != null && element.HasAttribute("for") && element.HasAttribute("value"))
                        {
                            string conditionsFor = element.Attributes["for"].InnerText;
                            string conditionsString = element.Attributes["value"].InnerText;
                            ComponentDescriptionConditionCollection conditionCollection = ComponentDescriptionConditionCollection.Parse(conditionsString);

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
                    ComponentDescriptionConditionCollection conditionCollection = new ComponentDescriptionConditionCollection();
                    List<ConnectionDescription> connections = new List<ConnectionDescription>();

                    if ((connectionGroupNode as XmlElement).HasAttribute("conditions"))
                    {
                        string[] conditions = connectionGroupNode.Attributes["conditions"].InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string condition in conditions)
                            conditionCollection.Add(ComponentDescriptionCondition.Parse(condition));
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
                    ComponentDescriptionConditionCollection conditionCollection = new ComponentDescriptionConditionCollection();
                    List<IRenderCommand> commands = new List<IRenderCommand>();

                    if ((renderNode as XmlElement).HasAttribute("conditions"))
                    {
                        string[] conditions = renderNode.Attributes["conditions"].InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string condition in conditions)
                            conditionCollection.Add(ComponentDescriptionCondition.Parse(condition));
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
                            Color fillColour = Colors.Transparent;
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
                                fillColour = Colors.Black;
                            double width = double.Parse(renderCommandNode.Attributes["width"].InnerText);
                            double height = double.Parse(renderCommandNode.Attributes["height"].InnerText);
                            Rectangle rectangle = new Rectangle(location, width, height, thickness, fillColour);
                            commands.Add(rectangle);
                        }
                        else if (renderCommandNode.Name.ToLowerInvariant() == "ellipse")
                        {
                            double thickness = 2d;
                            if ((renderCommandNode as XmlElement).HasAttribute("thickness"))
                                thickness = double.Parse(renderCommandNode.Attributes["thickness"].InnerText);
                            Color fillColour = Colors.Transparent;
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
                            Ellipse ellipse = new Ellipse(centre, radiusX, radiusY, thickness, fillColour);
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

                            string value = renderCommandNode.Attributes["value"].InnerText;

                            Text text = new Text(location, alignment, size, value);
                            commands.Add(text);
                        }
                        else if (renderCommandNode.Name.ToLowerInvariant() == "path")
                        {
                            double thickness = 2d;
                            if ((renderCommandNode as XmlElement).HasAttribute("thickness"))
                                thickness = double.Parse(renderCommandNode.Attributes["thickness"].InnerText);
                            ComponentPoint location;
                            if ((renderCommandNode as XmlElement).HasAttribute("start"))
                                location = new ComponentPoint(renderCommandNode.Attributes["start"].InnerText);
                            else
                            {
                                string x = renderCommandNode.Attributes["x"].InnerText;
                                string y = renderCommandNode.Attributes["y"].InnerText;
                                location = new ComponentPoint(x, y);
                            }

                            string data = renderCommandNode.Attributes["data"].InnerText;


                            string pathLetters = "mlhvcsqtaz";
                            Regex commandsRegex = new Regex("[" + pathLetters + pathLetters.ToUpperInvariant() + "] ?\\-?[0-9e,\\-. ]+");
                            Regex letterRegex = new Regex("[" + pathLetters + pathLetters.ToUpperInvariant() + "]");
                            Regex numberRegex = new Regex("[0-9e,\\-. ]+");
                            MatchCollection commandsMatch = commandsRegex.Matches(data);
                            List<IPathCommand> pathCommands = new List<IPathCommand>();
                            double lastX = 0;
                            double lastY = 0;
                            foreach (Match pathCommand in commandsMatch)
                            {
                                string letter = letterRegex.Match(pathCommand.Value).Value;
                                bool isAbsolute = (letter.ToLowerInvariant() != letter);

                                CommandType pCommand;
                                switch (letter.ToLowerInvariant())
                                {
                                    case "m":
                                        pCommand = CommandType.MoveTo;
                                        break;
                                    case "l":
                                        pCommand = CommandType.LineTo;
                                        break;
                                    case "h":
                                        pCommand = CommandType.LineTo;
                                        break;
                                    case "v":
                                        pCommand = CommandType.LineTo;
                                        break;
                                    case "c":
                                        pCommand = CommandType.CurveTo;
                                        break;
                                    case "s":
                                        pCommand = CommandType.SmoothCurveTo;
                                        break;
                                    case "q":
                                        pCommand = CommandType.QuadraticBeizerCurveTo;
                                        break;
                                    case "t":
                                        pCommand = CommandType.SmoothQuadraticBeizerCurveTo;
                                        break;
                                    case "a":
                                        pCommand = CommandType.EllipticalArcTo;
                                        break;
                                    case "z":
                                        pCommand = CommandType.ClosePath;
                                        break;
                                    default:
                                        continue;
                                }

                                if (pCommand == CommandType.LineTo || pCommand == CommandType.MoveTo || pCommand == CommandType.SmoothQuadraticBeizerCurveTo)
                                {
                                    Match numbersMatch = numberRegex.Match(pathCommand.Value);
                                    string[] numbers = numbersMatch.Value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    for (int o = 0; o + 2 <= numbers.Length; o+=2) // shorthand syntax
                                    {
                                        double xLocation = double.Parse(numbers[o + 0]);
                                        double yLocation = 0;
                                        if (letter.ToLowerInvariant() == "h" || letter.ToLowerInvariant() == "v")
                                        {
                                            if (letter.ToLowerInvariant() == "v")
                                            {
                                                yLocation = xLocation;
                                                xLocation = 0;
                                            }

                                            if (!isAbsolute)
                                            {
                                                xLocation += lastX;
                                                yLocation += lastY;
                                            }

                                            pathCommands.Add(new LineTo(xLocation, yLocation));
                                        }
                                        else
                                        {
                                            yLocation = double.Parse(numbers[o + 1]);
                                            if (!isAbsolute)
                                            {
                                                xLocation += lastX;
                                                yLocation += lastY;
                                            }

                                            switch (pCommand)
                                            {
                                                case CommandType.MoveTo:
                                                    pathCommands.Add(new MoveTo(xLocation, yLocation));
                                                    break;
                                                case CommandType.LineTo:
                                                    pathCommands.Add(new LineTo(xLocation, yLocation));
                                                    break;
                                                case CommandType.SmoothQuadraticBeizerCurveTo:
                                                    pathCommands.Add(null);
                                                    break;
                                            }
                                        }

                                        lastX = xLocation;
                                        lastY = yLocation;
                                    }
                                }
                                else if (pCommand == CommandType.SmoothCurveTo || pCommand == CommandType.QuadraticBeizerCurveTo)
                                {
                                    Match numbersMatch = numberRegex.Match(pathCommand.Value);
                                    string[] numbers = numbersMatch.Value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    double xA = double.Parse(numbers[0]);
                                    double yA = double.Parse(numbers[1]);
                                    double xB = double.Parse(numbers[2]);
                                    double yB = double.Parse(numbers[3]);
                                    if (!isAbsolute)
                                    {
                                        xA += lastX;
                                        yA += lastY;
                                        xB += lastX;
                                        yB += lastY;
                                    }

                                    switch (pCommand)
                                    {
                                        case CommandType.SmoothCurveTo:
                                            pathCommands.Add(new SmoothCurveTo());
                                            break;
                                        case CommandType.QuadraticBeizerCurveTo:
                                            pathCommands.Add(new QuadraticBeizerCurveTo(xA, yA, xB, yB));
                                            break;
                                    }

                                    lastX = xB;
                                    lastY = yB;
                                }
                                else if (pCommand == CommandType.CurveTo)
                                {
                                    Match numbersMatch = numberRegex.Match(pathCommand.Value);
                                    string[] numbers = numbersMatch.Value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    for (int o = 0; o + 6 <= numbers.Length; o+=6) // shorthand syntax
                                    {
                                        double xA = double.Parse(numbers[o + 0]);
                                        double yA = double.Parse(numbers[o + 1]);
                                        double xB = double.Parse(numbers[o + 2]);
                                        double yB = double.Parse(numbers[o + 3]);
                                        double xC = double.Parse(numbers[o + 4]);
                                        double yC = double.Parse(numbers[o + 5]);
                                        if (!isAbsolute)
                                        {
                                            xA += lastX;
                                            yA += lastY;
                                            xB += lastX;
                                            yB += lastY;
                                            xC += lastX;
                                            yC += lastY;
                                        }

                                        switch (pCommand)
                                        {
                                            case CommandType.CurveTo:
                                                pathCommands.Add(new CurveTo(xA, yA, xB, yB, xC, yC));
                                                break;
                                        }

                                        lastX = xC;
                                        lastY = yC;
                                    }
                                }
                                else if (pCommand == CommandType.EllipticalArcTo)
                                {
                                    Match numbersMatch = numberRegex.Match(pathCommand.Value);
                                    string[] numbers = numbersMatch.Value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    for (int o = 0; o + 7 <= numbers.Length; o += 7) // shorthand syntax
                                    {
                                        double rx = double.Parse(numbers[o + 0]);
                                        double ry = double.Parse(numbers[o + 1]);
                                        double xrotation = double.Parse(numbers[o + 2]);
                                        double islargearc = double.Parse(numbers[o + 3]);
                                        double sweep = double.Parse(numbers[o + 4]);
                                        double x = double.Parse(numbers[o + 5]);
                                        double y = double.Parse(numbers[o + 6]);
                                        if (!isAbsolute)
                                        {
                                            x += lastX;
                                            y += lastY;
                                        }

                                        switch (pCommand)
                                        {
                                            case CommandType.EllipticalArcTo:
                                                pathCommands.Add(new EllipticalArcTo(rx, ry, xrotation, islargearc == 0, sweep == 1, x, y));
                                                break;
                                        }

                                        lastX = x;
                                        lastY = y;
                                    }
                                }
                                else // close path
                                {
                                    //pathCommands.Add(new ClosePath());
                                }
                            }





                            commands.Add(new Path(location, thickness, Colors.Transparent, pathCommands));
                            //commands.Add(new Path(location, thickness, Colors.Transparent, data));
                        }
                    }

                    parsedRenderDescriptions.Add(new RenderDescription(conditionCollection, commands.ToArray()));
                }
                
                m_descriptions = new ComponentDescription[1];
                ComponentDescriptionMetadata metadata = new ComponentDescriptionMetadata();
                metadata.Type = "XML (*.xml)";
                if (version != null)
                    metadata.Version = new Version(version);
                metadata.Configurations.AddRange(componentConfigurations);
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