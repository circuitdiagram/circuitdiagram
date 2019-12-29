using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Autofac.Features.AttributeFilters;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Readers;
using CircuitDiagram.TypeDescriptionIO.Xml.Readers.RenderCommands;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Definitions
{
    public static class DefinitionsXmlLoaderExtensions
    {
        public const string FeatureName = "experimental.definitions.enabled";

        public static void UseDefinitions(this XmlLoader loader)
        {
            loader.RegisterFeature(FeatureName, builder =>
            {
                builder.RegisterType<ComponentPointWithDefinitionParser>().As<IComponentPointParser>();
                builder.RegisterType<DefinitionsSectionReader>().Named<IXmlSectionReader>(XmlLoader.ComponentNamespace.NamespaceName + ":definitions");
                builder.RegisterType<TextCommandWithDefinitionsReader>().Named<IRenderCommandReader>(XmlLoader.ComponentNamespace.NamespaceName + ":text");
            });
        }
    }
}
