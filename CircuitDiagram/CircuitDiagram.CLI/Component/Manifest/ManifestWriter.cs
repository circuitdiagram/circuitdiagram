// This file is part of Circuit Diagram.
// Copyright (c) 2017 Samuel Fisher
//  
// Circuit Diagram is free software; you can redistribute it and/or
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
// along with Circuit Diagram. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;

namespace CircuitDiagram.CLI.Component.Manifest
{
    class ManifestWriter
    {
        public static void WriteManifest(IList<IManifestEntry> compiledEntries, Stream destination)
        {
            var writer = XmlWriter.Create(destination, new XmlWriterSettings
            {
                Indent = true
            });

            writer.WriteStartDocument();
            writer.WriteStartElement("components");

            foreach (var entry in GroupManifestEntries(compiledEntries))
            {
                writer.WriteStartElement("component");
                writer.WriteAttributeString("name", entry.ComponentName);
                writer.WriteAttributeString("author", entry.Author);
                writer.WriteAttributeString("guid", entry.Guid.ToString());
                writer.WriteAttributeString("input", entry.Input);

                foreach (var metaEntry in entry.Metadata)
                {
                    writer.WriteStartElement("meta");
                    writer.WriteAttributeString("name", metaEntry.Key);
                    writer.WriteAttributeString("value", metaEntry.Value);
                    writer.WriteEndElement();
                }

                foreach (var output in entry.Outputs)
                {
                    writer.WriteStartElement("output");
                    writer.WriteAttributeString("format", output.Key);
                    writer.WriteValue(output.Value);
                    writer.WriteEndElement();
                }

                foreach(var configuration in entry.Configurations)
                {
                    writer.WriteStartElement("configuration");

                    if (configuration.Guid.HasValue)
                        writer.WriteAttributeString("guid", configuration.Guid.ToString());

                    writer.WriteAttributeString("name", configuration.Name);
                    writer.WriteAttributeString("input", configuration.Input);
                    
                    foreach(var output in configuration.Outputs)
                    {
                        writer.WriteStartElement("output");
                        writer.WriteAttributeString("format", output.Key);
                        writer.WriteValue(output.Value);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteEndDocument();

            writer.Flush();
            writer.Dispose();
        }

        private static IEnumerable<ComponentManifestItem> GroupManifestEntries(IList<IManifestEntry> compiledEntries)
        {
            return compiledEntries.Where(x => x is ComponentDescriptionManifestEntry).Cast<ComponentDescriptionManifestEntry>().Select(g =>
            {
                var configurations = g.ConfigurationOutputFiles.Select(x => new ConfigurationItem
                {
                    Input = g.InputFile,
                    Name = x.Key,
                    Outputs = x.Value,
                });

                var additionalConfigurations = compiledEntries.Where(x => x is ComponentConfigurationManifestEntry && x.ComponentGuid == g.ComponentGuid).Cast<ComponentConfigurationManifestEntry>().Select(x => new ConfigurationItem
                {
                    Guid = x.Guid,
                    Input = x.InputFile,
                    Name = x.ConfigurationName,
                    Outputs = x.OutputFiles,
                });

                return new ComponentManifestItem
                {
                    Guid = g.ComponentGuid,
                    Author = g.Author,
                    ComponentName = g.ComponentName,
                    Input = g.InputFile,
                    Metadata = g.Metadata,
                    Outputs = g.OutputFiles,
                    Configurations = configurations.Concat(additionalConfigurations).ToList(),
                };
            });
        }

        private class ComponentManifestItem
        {
            public string ComponentName { get; set; }
            public string Author { get; set; }
            public Guid Guid { get; set; }
            public string Input { get; set; }
            public IReadOnlyDictionary<string, string> Metadata { get; set; }
            public IReadOnlyDictionary<string, string> Outputs { get; set; }
            public IReadOnlyList<ConfigurationItem> Configurations { get; set; }
        }

        private class ConfigurationItem
        {
            public Guid? Guid { get; set; }
            public string Name { get; set; }
            public string Input { get; set; }
            public IReadOnlyDictionary<string, string> Outputs { get; set; }
        }
    }
}
