// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CircuitDiagram.Circuit;
using CircuitDiagram.Primitives;
using Ns = CircuitDiagram.Document.Namespaces;

namespace CircuitDiagram.Document.InternalWriter
{
    class MainDocumentWriter
    {
        private const string Version = "1.3";

        public void Write(CircuitDocument document, Stream stream)
        {
            var context = new WriterContext();

            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement(Ns.Document + "circuit",
                    new XAttribute("version", Version),
                    CreateMetadata(document, context),
                    CreateDefinitions(document, context),
                    CreateElements(document, context)));

            var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "\t"
            });

            xml.WriteTo(writer);
            writer.Flush();
        }

        private XElement CreateMetadata(CircuitDocument document, WriterContext context)
        {
            var metadata = new XElement(Ns.Document + "properties");

            if (document.Size != null)
            {
                metadata.Add(new XElement(Ns.Document + "width", document.Size.Width));
                metadata.Add(new XElement(Ns.Document + "height", document.Size.Height));
            }

            return metadata;
        }

        private XElement CreateDefinitions(CircuitDocument document, WriterContext context)
        {
            var componentSources = new XElement(Ns.Document + "definitions");

            var componentTypes = document.Elements.Where(x => x is Component)
                                         .Cast<Component>()
                                         .Select(c => c.Type)
                                         .Distinct();

            foreach (var source in componentTypes.GroupBy(x => x.Collection))
            {
                var sourceXml = new XElement(Ns.Document + "src");

                if (source.Key != null && source.Key != ComponentType.UnknownCollection)
                    sourceXml.SetAttributeValue("col", source.Key);

                foreach (var type in source)
                {
                    var typeXml = new XElement(Ns.Document + "add");
                    typeXml.SetAttributeValue("id", context.AssignId(type));

                    if (type.CollectionItem != null)
                        typeXml.SetAttributeValue("item", type.CollectionItem);

                    var tdComponentType = type as TypeDescriptionComponentType;

                    if (tdComponentType != null)
                    {
                        typeXml.SetAttributeValue(Ns.DocumentComponentDescriptions + "guid", tdComponentType.Id);
                    }

                    sourceXml.Add(typeXml);
                }

                componentSources.Add(sourceXml);
            }

            return componentSources;
        }

        private XElement CreateElements(CircuitDocument document, WriterContext context)
        {
            var documentXml = new XElement(Ns.Document + "elements");

            foreach (var component in document.Components)
            {
                var componentXml = new XElement(Ns.Document + "c");

                componentXml.SetAttributeValue("id", context.GetOrAssignId(component));

                var typeId = context.GetId(component.Type);
                componentXml.SetAttributeValue("tp", "{" + typeId + "}");

                // Layout
                var positionalComponent = component as PositionalComponent;
                if (positionalComponent != null)
                    WriteLayout(positionalComponent.Layout, componentXml);

                // Properties
                var properties = new XElement(Ns.Document + "prs");
                
                foreach (var property in component.Properties)
                {
                    properties.Add(new XElement(Ns.Document + "p",
                        new XAttribute("k", property.Key),
                        new XAttribute("v", property.Value)));
                }
                componentXml.Add(properties);

                // Connections
                var connections = new XElement(Ns.Document + "cns");
                foreach (var connection in component.Connections)
                {
                    connections.Add(new XElement(Ns.Document + "cn",
                        new XAttribute("id", context.GetOrAssignId(connection.Value.Connection)),
                        new XAttribute("pt", connection.Key)));
                }
                componentXml.Add(connections);

                documentXml.Add(componentXml);
            }

            foreach (var wire in document.Wires)
            {
                var wireXml = new XElement(Ns.Document + "w");
                WriteLayout(wire.Layout, wireXml);
                documentXml.Add(wireXml);
            }

            return documentXml;
        }

        private static void WriteLayout(LayoutInformation layout, XElement targetElement)
        {
            targetElement.SetAttributeValue("x", layout.Location.X);
            targetElement.SetAttributeValue("y", layout.Location.Y);

            targetElement.SetAttributeValue("o",
                layout.Orientation == Orientation.Horizontal
                    ? "h"
                    : "v");

            targetElement.SetAttributeValue("sz", layout.Size);

            var flipValue = "false";
            switch (layout.Flip)
            {
                case FlipState.Primary:
                    flipValue = "true";
                    break;
                case FlipState.Secondary:
                    flipValue = "s";
                    break;
                case FlipState.Both:
                    flipValue = "p+s";
                    break;
            }
            targetElement.SetAttributeValue("flp", flipValue);
        }
    }
}
