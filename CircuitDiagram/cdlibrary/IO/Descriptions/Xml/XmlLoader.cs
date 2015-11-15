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
using System.Xml.Linq;
using CircuitDiagram.Components.Description.Render;
using System.Windows.Media;
using CircuitDiagram.Components;
using CircuitDiagram.Render.Path;
using System.Text.RegularExpressions;
using CircuitDiagram.Render;
using CircuitDiagram.Components.Description;
using CircuitDiagram.Components.Conditions;
using CircuitDiagram.Components.Conditions.Parsers;
using System.Xml.XPath;

namespace CircuitDiagram.IO.Descriptions.Xml
{
    /// <summary>
    /// Loads component descriptions from an XML file.
    /// </summary>
    public class XmlLoader : IComponentDescriptionLoader
    {
        public readonly string ComponentNamespace = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/component/xml";

        ComponentDescription[] m_descriptions;
        XNamespace ns;

        public IEnumerable<LoadError> LoadErrors { get; private set; }

        public ComponentDescription[] GetDescriptions()
        {
            return m_descriptions;
        }

        public void Load(System.IO.Stream stream)
        {
            LoadContext lc = new LoadContext();

            try
            {
                string fileName = "file.xml";
                if (stream is System.IO.FileStream)
                    fileName = (stream as System.IO.FileStream).Name;
                lc.FileName = fileName;

                var reader = new XmlTextReader(stream);
                var root = XElement.Load(reader, LoadOptions.SetLineInfo);

                ns = XNamespace.Get(ComponentNamespace);
                lc.NS = ns;

                lc.NamespaceManager = new XmlNamespaceManager(reader.NameTable);
                lc.NamespaceManager.AddNamespace("cd", ComponentNamespace);

                // Read format version
                lc.FormatVersion = new Version(1, 2);
                if (root.Attribute("version") != null)
                    lc.FormatVersion = new Version(root.Attribute("version").Value);

                // Decide which condition parser to use
                if (lc.FormatVersion >= new Version(1, 1))
                {
                    ConditionFormat conditionParseFormat = new ConditionFormat();
                    if (lc.FormatVersion == new Version(1, 1))
                        conditionParseFormat.StatesUnderscored = true;
                    else
                        conditionParseFormat.StatesUnderscored = false;

                    lc.ConditionParser = new ConditionParser(conditionParseFormat);

                }
                else
                    lc.ConditionParser = new LegacyConditionParser();

                ComponentDescription description = new ComponentDescription();

                var declaration = root.XPathSelectElement("/cd:declaration", lc.NamespaceManager);
                ReadDeclarationSection(declaration, ns, lc, description);

                ReadConnectionsSection(declaration, lc, description);

                ReadRenderSection(declaration, lc, description);

                m_descriptions = new ComponentDescription[] { description };
            }
            catch (Exception ex)
            {
                lc.Errors.Add(new LoadError(lc.FileName, 0, 0, LoadErrorCategory.Error, "A fatal error occurred - '" + ex.Message + "'"));
                m_descriptions = new ComponentDescription[0];
            }
            finally
            {
                LoadErrors = lc.Errors;
            }
        }

        private void ReadDeclarationSection(XElement declaration, XNamespace ns, LoadContext lc, ComponentDescription description)
        {
            IXmlLineInfo line = declaration as IXmlLineInfo;

            // Read meta nodes
            foreach (var metaElement in declaration.Elements(ns + "meta"))
            {
                ReadMetaNode(metaElement, lc, description);
            }

            // Check all required metadata was set
            if (String.IsNullOrEmpty(description.ComponentName))
                lc.Errors.Add(new LoadError(lc.FileName, line.LineNumber, line.LinePosition, LoadErrorCategory.Error,
                    "Component name is required"));

            //Read properties
            List<ComponentProperty> properties = new List<ComponentProperty>();
            foreach (var propertyElement in declaration.Elements(ns + "property"))
            {
                ScanPropertyNode(propertyElement, lc);
            }
            foreach (var propertyElement in declaration.Elements(ns + "property"))
            {
                ComponentProperty property = ReadPropertyNode(propertyElement, lc);
                properties.Add(property);
            }
            description.Properties = properties.ToArray();

            // Read flags
            List<Conditional<FlagOptions>> flagOptions = new List<Conditional<FlagOptions>>();
            foreach (var flagGroup in declaration.Elements(ns + "flags"))
            {
                var flags = ReadFlagOptionNode(flagGroup, lc);
                if (flags != null)
                    flagOptions.Add(flags);
            }
            description.Flags = flagOptions.ToArray();

            // Read configurations
            var componentConfigurations = new List<ComponentConfiguration>();
            var configurations = declaration.Element(ns + "configurations");
            if (configurations != null)
            {
                foreach (var node in configurations.Elements(ns + "configuration"))
                {
                    ComponentConfiguration newConfiguration = ReadConfigurationNode(node, description);

                    componentConfigurations.Add(newConfiguration);
                }
            }
            description.Metadata.Configurations.AddRange(componentConfigurations);
        }

        private void ReadConnectionsSection(XElement declaration, LoadContext lc, ComponentDescription description)
        {
            List<ConnectionGroup> parsedConnectionGroups = new List<ConnectionGroup>();
            var connectionGroupNodes = declaration.XPathSelectElements("/cd:connections/cd:group", lc.NamespaceManager);
            foreach (var connectionGroupNode in connectionGroupNodes)
            {
                IConditionTreeItem conditionCollection = ConditionTree.Empty;
                List<ConnectionDescription> connections = new List<ConnectionDescription>();

                var conditionsAttribute = connectionGroupNode.Attribute("conditions");
                if (conditionsAttribute != null)
                {
                    IXmlLineInfo line = conditionsAttribute as IXmlLineInfo;

                    try
                    {
                        conditionCollection = lc.ConditionParser.Parse(connectionGroupNode.Attribute("conditions").Value, lc.ParseContext);
                    }
                    catch (ConditionFormatException ex)
                    {
                        lc.Errors.Add(new LoadError(lc.FileName, line.LineNumber, line.LinePosition + conditionsAttribute.Name.LocalName.Length + 2 + ex.PositionStart, LoadErrorCategory.Error, ex.Message));
                        continue;
                    }
                }

                foreach (var connectionNode in connectionGroupNode.Elements(ns + "connection"))
                {
                    ConnectionEdge edge = ConnectionEdge.None;
                    if (connectionNode.Attribute("edge") != null)
                    {
                        string edgeText = connectionNode.Attribute("edge").Value.ToLowerInvariant();
                        if (edgeText == "start")
                            edge = ConnectionEdge.Start;
                        else if (edgeText == "end")
                            edge = ConnectionEdge.End;
                        else if (edgeText == "both")
                            edge = ConnectionEdge.Both;
                    }
                    string connectionName = "#";
                    if (connectionNode.Attribute("name") != null)
                        connectionName = connectionNode.Attribute("name").Value;
                    connections.Add(new ConnectionDescription(new ComponentPoint(connectionNode.Attribute("start").Value), new ComponentPoint(connectionNode.Attribute("end").Value), edge, connectionName));
                }

                parsedConnectionGroups.Add(new ConnectionGroup(conditionCollection, connections.ToArray()));
            }

            description.Connections = parsedConnectionGroups.ToArray();
        }

        private void ReadRenderSection(XElement declaration, LoadContext lc, ComponentDescription description)
        {
            List<RenderDescription> parsedRenderDescriptions = new List<RenderDescription>();
            var renderDescriptions = declaration.XPathSelectElements("/cd:render/cd:group", lc.NamespaceManager);
            foreach (var renderNode in renderDescriptions)
            {
                IConditionTreeItem conditionCollection = ConditionTree.Empty;
                List<IRenderCommand> commands = new List<IRenderCommand>();

                var conditionsAttribute = renderNode.Attribute("conditions");
                if (conditionsAttribute != null)
                {
                    IXmlLineInfo line = conditionsAttribute as IXmlLineInfo;

                    try
                    {
                        conditionCollection = lc.ConditionParser.Parse(conditionsAttribute.Value, lc.ParseContext);
                    }
                    catch (ConditionFormatException ex)
                    {
                        lc.Errors.Add(new LoadError(lc.FileName, line.LineNumber, line.LinePosition + conditionsAttribute.Name.LocalName.Length + 2 + ex.PositionStart, LoadErrorCategory.Error, ex.Message));
                        continue;
                    }
                }

                foreach (var renderCommandNode in renderNode.Descendants())
                {
                    string commandType = renderCommandNode.Name.LocalName;
                    if (commandType == "line")
                    {
                        var command = new Line();
                        command.LoadFromXml(renderCommandNode, lc);
                        commands.Add(command);
                    }
                    else if (commandType == "rect")
                    {
                        var command = new Rectangle();
                        command.LoadFromXml(renderCommandNode);
                        commands.Add(command);
                    }
                    else if (commandType == "ellipse")
                    {
                        var command = new Ellipse();
                        command.LoadFromXml(renderCommandNode, lc);
                        commands.Add(command);
                    }
                    else if (commandType == "text")
                    {
                        var command = new Text();
                        command.LoadFromXml(renderCommandNode, lc);
                        commands.Add(command);
                    }
                    else if (commandType == "path")
                    {
                        var command = new RenderPath();
                        command.LoadFromXml(renderCommandNode, lc);
                        commands.Add(command);
                    }
                }

                parsedRenderDescriptions.Add(new RenderDescription(conditionCollection, commands.ToArray()));
            }

            description.RenderDescriptions = parsedRenderDescriptions.ToArray();
        }

        private static ComponentConfiguration ReadConfigurationNode(XElement configurationElement, ComponentDescription description)
        {
            string configName = configurationElement.Attribute("name").Value;
            string configValue = configurationElement.Attribute("value").Value;
            string configImplements = null;
            if (configurationElement.Attribute("implements") != null)
                configImplements = configurationElement.Attribute("implements").Value;

            ComponentConfiguration newConfiguration = new ComponentConfiguration(configImplements, configName, new Dictionary<string,PropertyUnion>());

            string[] setters = configValue.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string setter in setters)
            {
                string[] tempSplit = setter.Split(':');
                string propertyName = tempSplit[0];
                string value = tempSplit[1];

                // convert from string to proper type
                foreach (ComponentProperty property in description.Properties)
                {
                    if (propertyName == property.Name)
                    {
                        newConfiguration.Setters.Add(property.SerializedName, new PropertyUnion(value, property.Type));
                        break;
                    }
                }
            }
            return newConfiguration;
        }

        private void ScanPropertyNode(XElement propertyElement, LoadContext lc)
        {
            string propertyName = propertyElement.Attribute("name").Value;
            string type = propertyElement.Attribute("type").Value;
            string defaultValue = propertyElement.Attribute("default").Value;

            PropertyType propertyType;
            switch (type.ToLowerInvariant())
            {
                case "double":
                    propertyType = PropertyType.Decimal;
                    break;
                case "decimal":
                    propertyType = PropertyType.Decimal;
                    break;
                case "int":
                    propertyType = PropertyType.Integer;
                    break;
                case "bool":
                    propertyType = PropertyType.Boolean;
                    break;
                default:
                    propertyType = PropertyType.String;
                    break;
            }

            PropertyUnion propertyDefaultValue = new PropertyUnion(defaultValue, propertyType);

            lc.ParseContext.PropertyTypes.Add(propertyName, propertyDefaultValue.InternalType);
        }

        private ComponentProperty ReadPropertyNode(XElement propertyElement, LoadContext lc)
        {
            IXmlLineInfo elementLine = propertyElement as IXmlLineInfo;

            string propertyName = propertyElement.Attribute("name").Value;
            string type = propertyElement.Attribute("type").Value;
            string defaultValue = propertyElement.Attribute("default").Value;
            string serializeAs = propertyElement.Attribute("serialize").Value;
            string display = propertyElement.Attribute("display").Value;

            PropertyType propertyType;
            switch (type.ToLowerInvariant())
            {
                case "double":
                    propertyType = PropertyType.Decimal;
                    if (lc.FormatVersion >= new Version(1, 2))
                        lc.Errors.Add(new LoadError(lc.FileName, elementLine.LineNumber, elementLine.LinePosition, LoadErrorCategory.Warning,
                            "Property type 'double' is deprecated, use 'decimal' instead"));
                    break;
                case "decimal":
                    propertyType = PropertyType.Decimal;
                    break;
                case "int":
                    propertyType = PropertyType.Integer;
                    break;
                case "bool":
                    propertyType = PropertyType.Boolean;
                    break;
                default:
                    propertyType = PropertyType.String;
                    break;
            }

            PropertyUnion propertyDefaultValue = new PropertyUnion(defaultValue, propertyType);

            List<string> propertyOptions = null;
            if (type == "enum")
            {
                propertyOptions = new List<string>();
                var optionNodes = propertyElement.Elements(ns + "option");
                foreach (var optionNode in optionNodes)
                    propertyOptions.Add(optionNode.Value);
            }

            List<ComponentPropertyFormat> formatRules = new List<ComponentPropertyFormat>();
            if (propertyElement.Attribute("format") != null)
                formatRules.Add(new ComponentPropertyFormat(propertyElement.Attribute("format").Value, ConditionTree.Empty));
            else
            {
                var formatRuleNodes = propertyElement.XPathSelectElements("cd:formatting/cd:format", lc.NamespaceManager);
                foreach (var formatNode in formatRuleNodes)
                {
                    IXmlLineInfo line = formatNode as IXmlLineInfo;

                    IConditionTreeItem conditionCollection = ConditionTree.Empty;
                    if (formatNode.Attribute("conditions") != null)
                    {
                        try
                        {
                            conditionCollection = lc.ConditionParser.Parse(formatNode.Attribute("conditions").Value, lc.ParseContext);
                        }
                        catch (ConditionFormatException ex)
                        {
                            lc.Errors.Add(new LoadError(lc.FileName, line.LineNumber, line.LinePosition + formatNode.Name.LocalName.Length + 2 + ex.PositionStart, LoadErrorCategory.Error, ex.Message));
                            return null;
                        }
                    }

                    formatRules.Add(new ComponentPropertyFormat(formatNode.Attribute("value").Value, conditionCollection));
                }
            }

            Dictionary<PropertyOtherConditionType, IConditionTreeItem> otherConditions = new Dictionary<PropertyOtherConditionType, IConditionTreeItem>();
            var otherConditionsNodes = propertyElement.XPathSelectElements("cd:other/cd:conditions", lc.NamespaceManager);
            foreach (var otherConditionNode in otherConditionsNodes)
            {
                if (otherConditionNode != null && otherConditionNode.Attribute("for") != null && otherConditionNode.Attribute("value") != null)
                {
                    IXmlLineInfo line = otherConditionNode as IXmlLineInfo;

                    string conditionsFor = otherConditionNode.Attribute("for").Value;
                    string conditionsString = otherConditionNode.Attribute("value").Value;
                    IConditionTreeItem conditionCollection;

                    try
                    {
                        conditionCollection = lc.ConditionParser.Parse(conditionsString, lc.ParseContext);
                    }
                    catch (ConditionFormatException ex)
                    {
                        lc.Errors.Add(new LoadError(lc.FileName, line.LineNumber, line.LinePosition + otherConditionNode.Name.LocalName.Length + 2 + ex.PositionStart, LoadErrorCategory.Error, ex.Message));
                        return null;
                    }

                    if (Enum.IsDefined(typeof(PropertyOtherConditionType), conditionsFor))
                        otherConditions.Add((PropertyOtherConditionType)Enum.Parse(typeof(PropertyOtherConditionType), conditionsFor, true), conditionCollection);
                }
            }

            ComponentProperty property = new ComponentProperty(propertyName, serializeAs, display, propertyType, propertyDefaultValue, formatRules.ToArray(), otherConditions, (propertyOptions == null ? null : propertyOptions.ToArray()));

            return property;
        }

        private void ReadMetaNode(XElement metaElement, LoadContext lc, ComponentDescription description)
        {
            string metaName;
            if (!metaElement.GetAttribute("name", lc, out metaName))
                return;

            string metaValue;
            if (!metaElement.GetAttribute("value", lc, out metaValue))
                return;

            IXmlLineInfo line = metaElement as IXmlLineInfo;

            switch (metaName)
            {
                case "name":
                    description.ComponentName = metaValue;
                    break;
                case "canresize":
                    description.CanResize = metaValue.ToLower() != "false";
                    break;
                case "canflip":
                    description.CanFlip = metaValue.ToLower() != "false";
                    break;
                case "minsize":
                    double minSize;
                    if (double.TryParse(metaValue, out minSize))
                        description.MinSize = minSize;
                    else
                        lc.Errors.Add(new LoadError(lc.FileName, line.LineNumber, line.LinePosition, LoadErrorCategory.Error,
                            "Illegal value for attribute 'value' on <meta> tag 'minsize' (expected decimal)"));
                    break;
                case "version":
                    try
                    {
                        description.Metadata.Version = new Version(metaValue);
                    }
                    catch
                    {
                        lc.Errors.Add(new LoadError(lc.FileName, line.LineNumber, line.LinePosition, LoadErrorCategory.Error,
                            "Illegal value for attribute 'value' on <meta> tag 'version' (expected version number)"));
                    }
                    break;
                case "author":
                    description.Metadata.Author = metaValue;
                    break;
                case "additionalinformation":
                    description.Metadata.AdditionalInformation = metaValue;
                    break;
                case "implementset":
                    description.Metadata.ImplementSet = metaValue;
                    break;
                case "implementitem":
                    description.Metadata.ImplementItem = metaValue;
                    break;
                case "guid":
                    try
                    {
                        description.Metadata.GUID = new Guid(metaValue);
                    }
                    catch
                    {
                        lc.Errors.Add(new LoadError(lc.FileName, line.LineNumber, line.LinePosition, LoadErrorCategory.Error,
                            "Illegal value for attribute 'value' on <meta> tag 'guid' (expected GUID)"));
                    }
                    break;
                default:
                    description.Metadata.Entries.Add(metaName, metaValue);
                    break;
            }
        }

        private Conditional<FlagOptions> ReadFlagOptionNode(XElement flagElement, LoadContext lc)
        {
            IConditionTreeItem conditions = ConditionTree.Empty;
            var conditionsAttribute = flagElement.Attribute("conditions");
            if (conditionsAttribute != null)
            {
                IXmlLineInfo line = conditionsAttribute as IXmlLineInfo;

                try
                {
                    conditions = lc.ConditionParser.Parse(conditionsAttribute.Value, lc.ParseContext);
                }
                catch (ConditionFormatException ex)
                {
                    lc.Errors.Add(new LoadError(lc.FileName, line.LineNumber, line.LinePosition + conditionsAttribute.Name.LocalName.Length + 2 + ex.PositionStart, LoadErrorCategory.Error, ex.Message));
                    return null;
                }
            }

            FlagOptions theOptions = FlagOptions.None;
            foreach (var node in flagElement.Descendants(lc.NS + "option"))
                theOptions |= (FlagOptions)Enum.Parse(typeof(FlagOptions), node.Value, true);

            return new Conditional<FlagOptions>(theOptions, conditions);
        }
    }

    class LoadContext
    {
        public LoadContext()
        {
            Errors = new List<LoadError>();
            ParseContext = new ParseContext();
        }

        public XNamespace NS { get; set; }
        public string FileName { get; set; }
        public Version FormatVersion { get; set; }
        public IConditionParser ConditionParser { get; set; }
        public XmlNamespaceManager NamespaceManager { get; set; }
        public List<LoadError> Errors { get; set; }
        public ParseContext ParseContext { get; set; }
    }
}