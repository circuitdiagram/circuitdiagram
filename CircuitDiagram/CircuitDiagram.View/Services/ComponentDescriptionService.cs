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
using System.Windows.Media;
using CircuitDiagram.Circuit;
using CircuitDiagram.IO.Descriptions.Xml;
using CircuitDiagram.Render;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO;
using CircuitDiagram.TypeDescriptionIO.Binary;
using ComponentConfiguration = CircuitDiagram.Circuit.ComponentConfiguration;

namespace CircuitDiagram.View.Services
{
    class ComponentDescriptionService : DictionaryComponentDescriptionLookup, IComponentDescriptionService
    {
        private readonly IConfigurationValues configurationValues;

        private readonly Dictionary<Tuple<ComponentType, ComponentConfiguration>, MultiResolutionImage> icons =
            new Dictionary<Tuple<ComponentType, ComponentConfiguration>, MultiResolutionImage>();

        public ComponentDescriptionService(IConfigurationValues configurationValues)
        {
            this.configurationValues = configurationValues;
        }

        public void LoadDescriptions()
        {
            LoadCompiledComponents(configurationValues.ComponentsDirectories.Where(Directory.Exists));
        }

        public MultiResolutionImage GetIcon(IComponentTypeIdentifier identifier)
        {
            MultiResolutionImage icon;
            if (icons.TryGetValue(Tuple.Create(identifier.Type, identifier.Configuration), out icon))
                return icon;
            return null;
        }

        public IReadOnlyCollection<ComponentType> AvailableTypes => LookupDictionary.Keys;

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
                            description.Source = new ComponentDescriptionSource(file, new System.Collections.ObjectModel.ReadOnlyCollection<ComponentDescription>(new ComponentDescription[] { description }));
                            
                            var type = GetTypeFromDescription(description);
                            AddDescription(type, description);
                        }
                    }
                }
            }
        }

        private void LoadCompiledComponents(IEnumerable<string> directories)
        {
            foreach (string location in directories)
            {
                foreach (string file in Directory.GetFiles(location, "*.cdcom", SearchOption.TopDirectoryOnly))
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var binaryLoader = new BinaryDescriptionReader();
                        if (binaryLoader.Read(fs))
                        {
                            foreach (var description in binaryLoader.ComponentDescriptions)
                            {
                                description.Metadata.Location = ComponentDescriptionMetadata.LocationType.Installed;
                                description.Source = new ComponentDescriptionSource(file,
                                                                                    new System.Collections.ObjectModel.ReadOnlyCollection<ComponentDescription>(new ComponentDescription[] {description}));
                                
                                var type = GetTypeFromDescription(description);
                                AddDescription(type, description);

                                var icon = description.Metadata.Icon as MultiResolutionImage;
                                if (icon != null)
                                    icons.Add(Tuple.Create(type, (ComponentConfiguration)null), icon);
                            }
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
