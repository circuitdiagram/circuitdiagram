using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using CircuitDiagram.Circuit;
using CircuitDiagram.Render;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO.Binary;
using CircuitDiagram.TypeDescriptionIO.Xml;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.TypeDescriptionIO.Util
{
    public class DirectoryComponentDescriptionLookup : IComponentDescriptionLookup
    {
        private readonly DictionaryComponentDescriptionLookup internalLookup = new DictionaryComponentDescriptionLookup();

        public DirectoryComponentDescriptionLookup(ILoggerFactory loggerFactory, string directory, bool recursive)
            : this(loggerFactory, directory, recursive, new XmlLoader())
        {
        }

        public DirectoryComponentDescriptionLookup(ILoggerFactory loggerFactory, string directory, bool recursive, XmlLoader xmlLoader)
        {
            LoadXmlComponents(loggerFactory, new[] { directory }, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, xmlLoader);
            LoadBinaryComponents(loggerFactory, new[] { directory }, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public ComponentDescription GetDescription(ComponentType componentType)
        {
            return internalLookup.GetDescription(componentType);
        }

        public IEnumerable<ComponentDescription> GetAllDescriptions() => internalLookup.GetAllDescriptions();

        private void LoadXmlComponents(ILoggerFactory loggerFactory, string[] directories, SearchOption searchOption, XmlLoader xmlLoader)
        {
            var logger = loggerFactory.CreateLogger<DirectoryComponentDescriptionLookup>();

            foreach (string location in directories)
            {
                foreach (string file in Directory.GetFiles(location, "*.xml", searchOption))
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        if (xmlLoader.Load(fs, loggerFactory.CreateLogger<XmlLoader>(), out var description))
                        {
                            description.Metadata.Location = ComponentDescriptionMetadata.LocationType.Installed;
                            description.Source = new ComponentDescriptionSource(file);

                            foreach (var type in description.GetComponentTypes())
                                internalLookup.AddDescription(type, description);
                        }
                        else
                        {
                            logger.LogError($"Failed to load {file}");
                        }
                    }
                }
            }
        }

        private void LoadBinaryComponents(ILoggerFactory loggerFactory, string[] directories, SearchOption searchOption)
        {
            var logger = loggerFactory.CreateLogger<DirectoryComponentDescriptionLookup>();

            var descriptionReader = new BinaryDescriptionReader();
            foreach (string location in directories)
            {
                foreach (string file in Directory.GetFiles(location, "*.cdcom", searchOption))
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        if (!descriptionReader.Read(fs))
                        {
                            logger.LogError($"Failed to load {file}");
                            continue;
                        }

                        foreach (var description in descriptionReader.ComponentDescriptions)
                        {
                            description.Metadata.Location = ComponentDescriptionMetadata.LocationType.Installed;
                            description.Source = new ComponentDescriptionSource(file);

                            foreach (var type in description.GetComponentTypes())
                                internalLookup.AddDescription(type, description);
                        }
                    }
                }
            }
        }
    }
}
