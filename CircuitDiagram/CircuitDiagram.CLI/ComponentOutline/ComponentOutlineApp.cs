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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using CircuitDiagram.TypeDescriptionIO.Xml;
using System.Text.Json;

namespace CircuitDiagram.CLI.ComponentOutline
{
    static class ComponentOutlineApp
    {
        public static int Run(Options options)
        {
            if (!File.Exists(options.Input))
            {
                Console.WriteLine("Input file does not exist.");
                return 1;
            }

            var loader = new XmlLoader();
            using (var fs = File.OpenRead(options.Input))
            {
                if (!loader.Load(fs, out var description))
                {
                    Console.WriteLine("Component is invalid.");
                    return 1;
                }

                var outline = new ComponentOutline
                {
                    Configurations = description.Metadata.Configurations.Select(x => new OutlineConfiguration
                    {
                        Name = x.Name,
                    }).ToList(),
                    Properties = description.Properties.Select(x => new OutlineProperty
                    {
                        Name = x.Name,
                        Type = x.Type.ToString(),
                        EnumOptions = x.EnumOptions,
                    }).ToList(),
                };

                using (var output = Console.OpenStandardOutput())
                {
                    var writer = new Utf8JsonWriter(output);
                    var jsonOptions = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    };
                    JsonSerializer.Serialize(writer, outline, jsonOptions);
                }
            }

            return 0;
        }

        private class ComponentOutline
        {
            public IList<OutlineProperty> Properties { get; set; }

            public IList<OutlineConfiguration> Configurations { get; set; }
        }

        private class OutlineProperty
        {
            public string Name { get; set; }

            public string Type { get; set; }

            public IList<string> EnumOptions { get; set; }
        }

        public class OutlineConfiguration
        {
            public string Name { get; set; }
        }

        [Verb("component-outline", HelpText = "Output a JSON component outline.")]
        public class Options
        {
            [Value(0, Required = true, HelpText = "Path to component.")]
            public string Input { get; set; }
        }
    }
}
