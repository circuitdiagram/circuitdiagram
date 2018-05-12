using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Autofac.Features.AttributeFilters;
using CircuitDiagram.TypeDescriptionIO.Xml.Experimental.Definitions.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Readers;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Experimental.Definitions
{
    public static class DefinitionsXmlLoaderExtensions
    {
        public const string FeatureName = "experimental.definitions.enabled";

        public static void UseDefinitions(this XmlLoader loader)
        {
            loader.RegisterFeature(FeatureName, builder =>
            {
                builder.RegisterType<ComponentPointTemplateParser>().WithAttributeFiltering().As<IComponentPointTemplateParser>();
                builder.RegisterType<ComponentPointTemplateParser>().WithAttributeFiltering().As<IComponentPointParser>();
                builder.RegisterType<DefinitionsSectionReader>().Named<IXmlSectionReader>(XmlLoader.ComponentNamespace.NamespaceName + ":definitions");
                builder.RegisterType<RenderSectionWithDefinitionsReader>().Named<IXmlSectionReader>(XmlLoader.ComponentNamespace.NamespaceName + ":render");
            });
        }
    }
}
