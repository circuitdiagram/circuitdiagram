// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2019  Samuel Fisher
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

using CircuitDiagram.Circuit;
using CircuitDiagram.Render;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CircuitDiagram.TypeDescription.Definition
{
    public class ConfigurationDefinitionReader
    {
        private readonly IComponentDescriptionLookup componentDescriptionLookup;
        IDeserializer deserializer;

        public ConfigurationDefinitionReader(IComponentDescriptionLookup componentDescriptionLookup)
        {
            this.componentDescriptionLookup = componentDescriptionLookup;
             deserializer = new DeserializerBuilder().WithNamingConvention(new CamelCaseNamingConvention()).IgnoreUnmatchedProperties().Build();
        }

        public ConfigurationDefinition ReadDefinition(Stream input)
        {
            var yaml = deserializer.Deserialize<YamlTemplate>(new StreamReader(input));

            var guid = Guid.Parse(yaml.Metadata.Guid);
            var descriptionGuid = Guid.Parse(yaml.Template.Guid);

            var description = componentDescriptionLookup.GetDescription(new TypeDescriptionComponentType(descriptionGuid, ComponentType.Unknown(yaml.Metadata.Name)));
            if (description == null)
                throw new InvalidOperationException("Base description for template not found.");

            var metadata = new DefinitionMetadata
            {
                Guid = guid,
                Description = yaml.Metadata.Description,
                Name = yaml.Metadata.Name,
                Version = yaml.Metadata.Version,
            };

            var properties = new Dictionary<PropertyName, PropertyValue>();
            foreach (var setter in yaml.Properties)
            {
                var componentProperty = description.Properties.FirstOrDefault(x => x.Name == setter.Name);
                if (componentProperty == null)
                    throw new InvalidOperationException($"Property '{setter.Name}' does not exist on component type '{description.ComponentName}'");

                PropertyValue value;
                switch (componentProperty.Type)
                {
                    case PropertyType.Boolean:
                    {
                        value = new PropertyValue((bool)setter.Value);
                        break;
                    }
                    case PropertyType.Decimal:
                    {
                        value = new PropertyValue((double)setter.Value);
                        break;
                    }
                    case PropertyType.Integer:
                    {
                        value = new PropertyValue((int)setter.Value);
                        break;
                    }
                    case PropertyType.Enum:
                    case PropertyType.String:
                    {
                        value = new PropertyValue((string)setter.Value);
                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException("Unknown property type.");
                    }
                }

                properties.Add(componentProperty.SerializedName, value);
            }

            var configuration = new ComponentConfiguration(null, yaml.Metadata.Name, properties);

            return new ConfigurationDefinition(metadata, description, configuration);
        }
    }

    class YamlTemplate
    {
        public string Kind { get; set; }
        public YamlMetadataSection Metadata { get; set; }
        public YamlTemplateSection Template { get; set; }
        public List<YamlProperty> Properties { get; set; }
    }

    class YamlMetadataSection
    {
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
    }

    class YamlTemplateSection
    {
        public string Name { get; set; }
        public string Guid { get; set; }
    }

    class YamlProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}
