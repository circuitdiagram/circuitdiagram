using CircuitDiagram.Circuit;
using CircuitDiagram.Render;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CircuitDiagram.TypeDescription.Template
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

            var descriptionGuid = Guid.Parse(yaml.Template.Guid);

            var description = componentDescriptionLookup.GetDescription(new TypeDescriptionComponentType(descriptionGuid, ComponentType.Unknown(yaml.Metadata.Name)));
            if (description == null)
                throw new InvalidOperationException("Base description for template not found.");

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

            return new ConfigurationDefinition(description, configuration);
        }
    }

    class YamlTemplate
    {
        public YamlMetadataSection Metadata { get; set; }
        public YamlTemplateSection Template { get; set; }
        public List<YamlProperty> Properties { get; set; }
    }

    class YamlMetadataSection
    {
        public string Name { get; set; }
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
