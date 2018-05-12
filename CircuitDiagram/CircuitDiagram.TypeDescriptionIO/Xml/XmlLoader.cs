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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO.Xml.Features;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Readers;
using CircuitDiagram.TypeDescriptionIO.Xml.Sections;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.TypeDescriptionIO.Xml
{
    /// <summary>
    /// Loads component descriptions from an XML file.
    /// </summary>
    public class XmlLoader
    {
        public static readonly XNamespace ComponentNamespace = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/component/xml";

        private readonly ContainerBuilder builder;

        private readonly Lazy<IContainer> container;

        private readonly Dictionary<string, Action<ContainerBuilder>> features = new Dictionary<string, Action<ContainerBuilder>>();
        
        public XmlLoader()
        {
            builder = new ContainerBuilder();

            builder.RegisterType<VersionedConditionParser>().As<IConditionParser>().InstancePerLifetimeScope();
            builder.RegisterType<ComponentPointParser>().As<IComponentPointParser>().Named<IComponentPointParser>("default").InstancePerLifetimeScope();
            builder.RegisterType<DeclarationSectionReader>().Named<IXmlSectionReader>(ComponentNamespace.NamespaceName + ":declaration").InstancePerLifetimeScope();
            builder.RegisterType<ConnectionsSectionReader>().Named<IXmlSectionReader>(ComponentNamespace.NamespaceName + ":connections").InstancePerLifetimeScope();
            builder.RegisterType<RenderSectionReader>().Named<IXmlSectionReader>(ComponentNamespace.NamespaceName + ":render").InstancePerLifetimeScope();

            container = new Lazy<IContainer>(() => builder.Build());
        }

        public void Configure(Action<ContainerBuilder> configure)
        {
            if (container.IsValueCreated)
                throw new InvalidOperationException("Configure cannot be called after a component description has already been loaded.");

            configure(builder);
        }

        public void RegisterFeature(string key, Action<ContainerBuilder> configure)
        {
            features.Add(key, configure);
        }

        public bool Load(Stream stream, out ComponentDescription description)
        {
            return Load(stream, new NullXmlLoadLogger(), out description);
        }

        public bool Load(Stream stream, ILogger logger, out ComponentDescription description)
        {
            return Load(stream, new XmlLoadLogger(logger, (stream as FileStream)?.Name), out description);
        }
        
        public bool Load(Stream stream, IXmlLoadLogger logger, out ComponentDescription description)
        {
            description = new ComponentDescription();

            if (stream is FileStream fs)
                description.Source = new ComponentDescriptionSource(fs.Name);

            var errorCheckingLogger = new ErrorCheckingLogger(logger);
            var sectionRegistry = new SectionRegistry();

            try
            {
                var root = XElement.Load(stream, LoadOptions.SetLineInfo);
                ReadHeader(root, description);

                var featureSwitcher = new FeatureSwitcher();

                // Read declaration
                var declaration = root.Element(ComponentNamespace + "declaration");
                if (declaration == null)
                {
                    logger.LogError(root, "Missing required element: 'declaration'");
                    return false;
                }
                var declarationReader = container.Value.ResolveNamed<IXmlSectionReader>(declaration.Name.NamespaceName + ":" + declaration.Name.LocalName,
                                                                                        new TypedParameter(typeof(IXmlLoadLogger), errorCheckingLogger),
                                                                                        new TypedParameter(typeof(FeatureSwitcher), featureSwitcher));
                declarationReader.ReadSection(declaration, description);

                var descriptionInstance = description;
                var scope = container.Value.BeginLifetimeScope(configure =>
                {
                    configure.RegisterInstance(errorCheckingLogger).As<IXmlLoadLogger>();
                    configure.RegisterInstance(sectionRegistry).As<ISectionRegistry>();
                    configure.RegisterInstance(descriptionInstance);
                    configure.RegisterInstance(featureSwitcher).As<IFeatureSwitcher>();

                    foreach (var feature in features)
                    {
                        if (featureSwitcher.IsFeatureEnabled(feature.Key, out var featureSourceElement))
                        {
                            logger.Log(LogLevel.Information, featureSourceElement, $"Feature enabled: {feature.Key}");
                            feature.Value(configure);
                        }
                    }
                });

                try
                {
                    // Read remaining elements
                    foreach (var element in root.Elements().Except(new[] { declaration }))
                    {
                        var sectionReader = scope.ResolveOptionalNamed<IXmlSectionReader>(element.Name.NamespaceName + ":" + element.Name.LocalName);
                        sectionReader?.ReadSection(element, description);
                    }

                    return !errorCheckingLogger.HasErrors;
                }
                finally
                {
                    scope.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, new FileRange(1, 1, 1, 2), ex.Message, ex);
                return false;
            }
        }

        private void ReadHeader(XElement root, ComponentDescription description)
        {
            // Read format version
            var formatVersion = new Version(1, 2);
            if (root.Attribute("version") != null)
                formatVersion = new Version(root.Attribute("version").Value);

            description.Metadata.FormatVersion = formatVersion;
            description.Metadata.Type = $"XML v{formatVersion.ToString(2)}";
        }
    }
}
