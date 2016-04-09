using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CircuitDiagram.IO.Data;
using Ns = CircuitDiagram.Document.Namespaces;

namespace CircuitDiagram.Document.InternalWriter
{
    class MainDocumentWriter
    {
        private const string Version = "1.2";

        public Formatting Formatting { get; set; }

        public void Write(CircuitDocument document, Stream stream)
        {
            var context = new WriterContext();
            
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement(Ns.Document + "circuit",
                    new XAttribute("version", Version),
                    CreateMetadata(document, context),
                    CreateComponentSources(document, context),
                    CreateDocument(document, context),
                    CreateLayout(document, context)));

            var writer = new XmlTextWriter(stream, Encoding.UTF8)
            {
                Formatting = Formatting
            };

            xml.WriteTo(writer);
            writer.Flush();
        }

        private XElement CreateMetadata(CircuitDocument document, WriterContext context)
        {
            var metadata = new XElement(Ns.Document + "metadata");

            if (document.Size != null)
            {
                metadata.Add(new XElement(Ns.Document + "width", document.Size.Width));
                metadata.Add(new XElement(Ns.Document + "height", document.Size.Height));
            }

            return metadata;
        }

        private XElement CreateComponentSources(CircuitDocument document, WriterContext context)
        {
            var componentSources = new XElement(Ns.Document + "componentsources");

            var componentTypes = document.Elements.Where(x => x is Component)
                                         .Cast<Component>()
                                         .Select(c => c.Type)
                                         .Distinct();
            
            foreach (var source in componentTypes.GroupBy(x => x.Collection))
            {
                var sourceXml = new XElement(Ns.Document + "source");
                sourceXml.SetAttributeValue("definitions", source.Key.Value);

                foreach (var type in source)
                {
                    var typeXml = new XElement(Ns.Document + "add");
                    typeXml.SetAttributeValue("id", context.AssignId(type));
                    typeXml.SetAttributeValue("name", type.Name);
                    typeXml.SetAttributeValue("item", type.CollectionItem);

                    // Only write configurations that are actually used in this document
                    foreach (var configuration in type.Configurations.Where(cf =>
                             document.Components.Any(cp => cp.Configuration == cf)))
                    {
                        typeXml.Add(new XElement(Ns.Document + "configuration",
                            new XAttribute("name", configuration.Name),
                            new XAttribute("implements", configuration.Implements)));
                    }

                    sourceXml.Add(typeXml);
                }

                componentSources.Add(sourceXml);
            }

            return componentSources;
        }

        private XElement CreateDocument(CircuitDocument document, WriterContext context)
        {
            var documentXml = new XElement(Ns.Document + "document");

            foreach (var component in document.Components)
            {
                var componentXml = new XElement(Ns.Document + "component");

                componentXml.SetAttributeValue("id", context.GetOrAssignId(component));
                componentXml.SetAttributeValue("type", context.GetId(component.Type));

                // Properties
                var properties = new XElement(Ns.Document + "properties");

                if (component.Configuration != null)
                    properties.SetAttributeValue("configuration", component.Configuration.Name);

                foreach (var property in component.Properties)
                {
                    properties.Add(new XElement(Ns.Document + "property",
                        new XAttribute("name", property.Id),
                        new XAttribute("value", property.Value)));
                }
                componentXml.Add(properties);

                // Connections
                var connections = new XElement(Ns.Document + "connections");
                foreach (var connection in component.Connections)
                {
                    connections.Add(new XElement(Ns.Document + "connection",
                        new XAttribute("id", context.GetOrAssignId(connection.Connection)),
                        new XAttribute("point", connection.Name)));
                }
                componentXml.Add(connections);

                documentXml.Add(componentXml);
            }

            return documentXml;
        }

        private XElement CreateLayout(CircuitDocument document, WriterContext context)
        {
            var layout = new XElement(Ns.Document + "layout");

            foreach (var pe in document.PositionalElements)
            {
                string elName = pe is Component ? "component" : "wire";

                var xmlPe = new XElement(Ns.Document + elName);

                if (pe is Component)
                    xmlPe.SetAttributeValue("id", context.GetOrAssignId(pe));

                xmlPe.SetAttributeValue("x", pe.Layout.Location.X);
                xmlPe.SetAttributeValue("y", pe.Layout.Location.Y);

                if (pe.Layout.Size.HasValue)
                    xmlPe.SetAttributeValue("size", pe.Layout.Size.Value);

                if (pe.Layout.Orientation.HasValue)
                    xmlPe.SetAttributeValue("orientation", pe.Layout.Orientation.Value.ToString().ToLowerInvariant());

                layout.Add(xmlPe);
            }

            return layout;
        }
    }
}
