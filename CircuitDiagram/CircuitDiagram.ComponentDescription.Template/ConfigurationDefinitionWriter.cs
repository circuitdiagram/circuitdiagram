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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CircuitDiagram.TypeDescription.Definition
{
    public class ConfigurationDefinitionWriter
    {
        ISerializer serializer;

        public ConfigurationDefinitionWriter()
        {
            serializer = new SerializerBuilder().WithNamingConvention(new CamelCaseNamingConvention()).Build();
        }

        public void WriteDefinition(ConfigurationDefinition definition, Stream destination)
        {
            var yamlTemplate = new YamlTemplate
            {
                Kind = "ComponentConfiguration/v1",
                Metadata = new YamlMetadataSection
                {
                    Guid = definition.Metadata.Guid.ToString(),
                    Name = definition.Configuration.Name,
                    Description = definition.Metadata.Description,
                },
                Template = new YamlTemplateSection
                {
                    Guid = definition.ComponentDescription.Metadata.GUID.ToString(),
                    Name = definition.ComponentDescription.ComponentName,
                },
                Properties = definition.Configuration.Setters.Select(x =>
                {
                    var property = definition.ComponentDescription.Properties.First(p => p.SerializedName == x.Key);
                    object value = null;
                    x.Value.Match(v => value = v, v => value = v, v => value = v);

                    return new YamlProperty
                    {
                        Name = property.Name,
                        Value = value,
                    };
                }).ToList(),
            };

            using (var writer = new StreamWriter(destination))
            {
                serializer.Serialize(writer, yamlTemplate);
                writer.Flush();
            }
        }
    }
}
