using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using CircuitDiagram.Circuit;
using CircuitDiagram.IO.Descriptions.Xml;
using CircuitDiagram.Render;
using CircuitDiagram.TypeDescription;
using ComponentConfiguration = CircuitDiagram.Circuit.ComponentConfiguration;

namespace CircuitDiagram.TypeDescriptionIO.Util
{
    public class DirectoryComponentDescriptionLookup : IComponentDescriptionLookup
    {
        private readonly DictionaryComponentDescriptionLookup internalLookup = new DictionaryComponentDescriptionLookup();

        public DirectoryComponentDescriptionLookup(params string[] directories)
        {
            LoadXmlComponents(directories);
        }

        public ComponentDescription GetDescription(ComponentType componentType)
        {
            return internalLookup.GetDescription(componentType);
        }

        private void LoadXmlComponents(string[] directories)
        {
            var xmlLoader = new XmlLoader();
            foreach (string location in directories)
            {
                foreach (string file in Directory.GetFiles(location, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        xmlLoader.Load(fs);
                        if (!xmlLoader.LoadErrors.Any())
                        {
                            ComponentDescription description = xmlLoader.GetDescriptions()[0];
                            description.Metadata.Location = ComponentDescriptionMetadata.LocationType.Installed;
                            description.Source = new ComponentDescriptionSource(file, new ReadOnlyCollection<ComponentDescription>(new[] { description }));

                            var type = GetTypeFromDescription(description);
                            internalLookup.AddDescription(type, description);
                        }
                    }
                }
            }
        }

        private ComponentType GetTypeFromDescription(ComponentDescription description)
        {
            var collection = !string.IsNullOrEmpty(description.Metadata.ImplementSet) ? new ComponentTypeCollection(new Uri(description.Metadata.ImplementSet)) : null;
            var collectionItem = !string.IsNullOrEmpty(description.Metadata.ImplementItem) ? new ComponentTypeCollectionItem(description.Metadata.ImplementItem) : null;
            var properties = description.Properties.Select(p => p.SerializedName);
            var connections = description.Connections.SelectMany(c => c.Value)
                                         .Select(c => c.Name)
                                         .Distinct()
                                         .Select(c => c);
            var configurations = description.Metadata.Configurations.Select(c => new ComponentConfiguration
            {
                Name = c.Name,
                Implements = c.ImplementationName
            });

            return new ComponentType(description.Metadata.GUID,
                                     collection,
                                     collectionItem,
                                     description.ComponentName,
                                     properties,
                                     connections,
                                     configurations);
        }
    }
}
