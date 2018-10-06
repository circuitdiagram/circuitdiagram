// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
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
using System.Xml.Linq;
using CircuitDiagram.Circuit;
using CircuitDiagram.Components;
using CircuitDiagram.IO;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Features;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.Conditions;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Readers
{
    class DeclarationSectionReader : IXmlSectionReader
    {
        private readonly IXmlLoadLogger logger;
        private readonly FeatureSwitcher featureSwitcher;
        private readonly IConditionParser conditionParser;

        public DeclarationSectionReader(IXmlLoadLogger logger, FeatureSwitcher featureSwitcher, IConditionParser conditionParser)
        {
            this.logger = logger;
            this.featureSwitcher = featureSwitcher;
            this.conditionParser = conditionParser;
        }

        public void ReadSection(XElement declarationElement, ComponentDescription description)
        {
            // Read meta nodes
            foreach (var metaElement in declarationElement.Elements(XmlLoader.ComponentNamespace + "meta"))
            {
                ReadMetaNode(metaElement, description);
            }

            // Check all required metadata was set
            if (string.IsNullOrEmpty(description.ComponentName))
                logger.LogError(declarationElement, "Component name is required.");

            // Read properties
            List<ComponentProperty> properties = new List<ComponentProperty>();
            foreach (var propertyElement in declarationElement.Elements(XmlLoader.ComponentNamespace + "property"))
            {
                ComponentProperty property = ScanPropertyNode(description, propertyElement);
                properties.Add(property);
            }
            description.Properties = properties.ToArray();

            properties.Clear();
            foreach (var propertyElement in declarationElement.Elements(XmlLoader.ComponentNamespace + "property"))
            {
                ComponentProperty property = ReadPropertyNode(description, propertyElement);
                properties.Add(property);
            }
            description.Properties = properties.ToArray();

            // Read flags
            List<Conditional<FlagOptions>> flagOptions = new List<Conditional<FlagOptions>>();
            foreach (var flagGroup in declarationElement.Elements(XmlLoader.ComponentNamespace + "flags"))
            {
                var flags = ReadFlagOptionNode(description, flagGroup);
                if (flags != null)
                    flagOptions.Add(flags);
            }

            description.Flags.AddRange(flagOptions);

            // Read configurations
            var componentConfigurations = new List<ComponentConfiguration>();
            var configurations = declarationElement.Element(XmlLoader.ComponentNamespace + "configurations");
            if (configurations != null)
            {
                foreach (var node in configurations.Elements(XmlLoader.ComponentNamespace + "configuration"))
                {
                    ComponentConfiguration newConfiguration = ReadConfigurationNode(node, description);

                    componentConfigurations.Add(newConfiguration);
                }
            }

            description.Metadata.Configurations.AddRange(componentConfigurations);
        }

        private static ComponentConfiguration ReadConfigurationNode(XElement configurationElement, ComponentDescription description)
        {
            string configName = configurationElement.Attribute("name").Value;
            string configValue = configurationElement.Attribute("value").Value;
            string configImplements = null;
            if (configurationElement.Attribute("implements") != null)
                configImplements = configurationElement.Attribute("implements").Value;

            ComponentConfiguration newConfiguration = new ComponentConfiguration(configImplements, configName, new Dictionary<PropertyName, PropertyValue>());

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
                        newConfiguration.Setters.Add(property.SerializedName, PropertyValue.Parse(value, property.Type.ToPropertyType()));
                        break;
                    }
                }
            }
            return newConfiguration;
        }

        private ComponentProperty ScanPropertyNode(ComponentDescription description, XElement propertyElement)
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

            var propertyDefaultValue = PropertyValue.Parse(defaultValue, propertyType.ToPropertyType());

            return new ComponentProperty(propertyName, null, null, propertyType, propertyDefaultValue, null, null);
        }

        private ComponentProperty ReadPropertyNode(ComponentDescription description, XElement propertyElement)
        {
            string propertyName = propertyElement.Attribute("name").Value;
            string type = propertyElement.Attribute("type").Value;
            string defaultValue = propertyElement.Attribute("default").Value;
            string serializeAs = propertyElement.Attribute("serialize").Value;
            string display = propertyElement.Attribute("display").Value;

            PropertyType propertyType;
            switch (type.ToLowerInvariant())
            {
                case "double":
                    // TODO: Add warning
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

            var propertyDefaultValue = PropertyValue.Parse(defaultValue, propertyType.ToPropertyType());

            List<string> propertyOptions = null;
            if (type == "enum")
            {
                propertyOptions = new List<string>();
                var optionNodes = propertyElement.Elements(propertyElement.GetDefaultNamespace() + "option");
                foreach (var optionNode in optionNodes)
                    propertyOptions.Add(optionNode.Value);
            }

            List<ComponentPropertyFormat> formatRules = new List<ComponentPropertyFormat>();
            if (propertyElement.Attribute("format") != null)
                formatRules.Add(new ComponentPropertyFormat(propertyElement.Attribute("format").Value, ConditionTree.Empty));
            else
            {
                var formatRuleNodes = propertyElement.Elements(propertyElement.GetDefaultNamespace() + "formatting")
                                                     .SelectMany(x => x.Elements(propertyElement.GetDefaultNamespace() + "format"));
                foreach (var formatNode in formatRuleNodes)
                {
                    IConditionTreeItem conditionCollection = ConditionTree.Empty;

                    var conditionsAttribute = formatNode.Attribute("conditions");
                    if (conditionsAttribute != null)
                        conditionParser.Parse(conditionsAttribute, description, logger, out conditionCollection);

                    formatRules.Add(new ComponentPropertyFormat(formatNode.Attribute("value").Value, conditionCollection));
                }
            }

            Dictionary<PropertyOtherConditionType, IConditionTreeItem> otherConditions = new Dictionary<PropertyOtherConditionType, IConditionTreeItem>();
            var otherConditionsNodes = propertyElement.Elements(propertyElement.GetDefaultNamespace() + "other")
                                                      .SelectMany(x => x.Elements(propertyElement.GetDefaultNamespace() + "conditions"));
            foreach (var otherConditionNode in otherConditionsNodes)
            {
                if (otherConditionNode?.Attribute("for") != null && otherConditionNode.Attribute("value") != null)
                {
                    string conditionsFor = otherConditionNode.Attribute("for").Value;
                    if (!conditionParser.Parse(otherConditionNode.Attribute("value"), description, logger, out var conditionCollection))
                        continue;
                    
                    if (Enum.IsDefined(typeof(PropertyOtherConditionType), conditionsFor))
                        otherConditions.Add((PropertyOtherConditionType)Enum.Parse(typeof(PropertyOtherConditionType), conditionsFor, true), conditionCollection);
                }
            }

            ComponentProperty property = new ComponentProperty(propertyName, serializeAs, display, propertyType, propertyDefaultValue, formatRules.ToArray(), otherConditions, (propertyOptions == null ? null : propertyOptions.ToArray()));

            return property;
        }

        private void ReadMetaNode(XElement metaElement, ComponentDescription description)
        {
            if (!metaElement.GetAttributeValue("name", logger, out var metaName))
                return;

            if (!metaElement.GetAttributeValue("value", logger, out var metaValue))
                return;
            
            switch (metaName)
            {
                case "name":
                    description.ComponentName = metaValue;
                    break;
                case "canresize":
                    if (description.Metadata.FormatVersion < new Version(1, 3))
                        description.SetDefaultFlag(FlagOptions.NoResize, metaValue.ToLowerInvariant() == "false");
                    break;
                case "canflip":
                    if (description.Metadata.FormatVersion < new Version(1, 3))
                        description.SetDefaultFlag(FlagOptions.FlipPrimary, metaValue.ToLowerInvariant() != "false");
                    break;
                case "minsize":
                {
                    if (double.TryParse(metaValue, out var minSize))
                        description.MinSize = minSize;
                    else
                        logger.LogError(metaElement, "Illegal value for attribute 'value' on <meta> tag 'minsize' (expected decimal)");
                    break;
                }
                case "version":
                {
                    try
                    {
                        description.Metadata.Version = new Version(metaValue);
                    }
                    catch
                    {
                        logger.LogError(metaElement, "Illegal value for attribute 'value' on <meta> tag 'version' (expected version number)");
                    }

                    break;
                }
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
                        logger.LogError(metaElement, "Illegal value for attribute 'value' on <meta> tag 'guid' (expected GUID)");
                    }
                    break;
                default:
                {
                    description.Metadata.Entries.Add(metaName, metaValue);
                    if (metaValue.ToLowerInvariant() == "true")
                        featureSwitcher.EnableFeatureCandidate(metaName, metaElement);
                    break;
                }
            }
        }

        private Conditional<FlagOptions> ReadFlagOptionNode(ComponentDescription description, XElement flagElement)
        {
            IConditionTreeItem conditions = ConditionTree.Empty;
            var conditionsAttribute = flagElement.Attribute("conditions");
            if (conditionsAttribute != null)
                conditionParser.Parse(conditionsAttribute, description, logger, out conditions);

            FlagOptions theOptions = FlagOptions.None;
            foreach (var node in flagElement.Elements(flagElement.GetDefaultNamespace() + "option"))
                theOptions |= (FlagOptions)Enum.Parse(typeof(FlagOptions), node.Value, true);

            return new Conditional<FlagOptions>(theOptions, conditions);
        }
    }
}
