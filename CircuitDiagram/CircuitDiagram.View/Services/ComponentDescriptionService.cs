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
using CircuitDiagram.Logging;
using CircuitDiagram.Render;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO;
using CircuitDiagram.TypeDescriptionIO.Binary;
using CircuitDiagram.TypeDescriptionIO.Util;
using CircuitDiagram.View.ToolboxView;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.View.Services
{
    class ComponentDescriptionService : DictionaryComponentDescriptionLookup, IComponentDescriptionService
    {
        private static readonly ILogger Log = LogManager.GetLogger<ComponentDescriptionService>();

        private readonly IConfigurationValues configurationValues;

        private readonly Dictionary<Tuple<Guid, string>, MultiResolutionImage> icons = new Dictionary<Tuple<Guid, string>, MultiResolutionImage>();

        public ComponentDescriptionService(IConfigurationValues configurationValues)
        {
            this.configurationValues = configurationValues;
        }

        public void LoadDescriptions()
        {
            foreach (var directory in configurationValues.ComponentsDirectories.Select(Path.GetFullPath))
            {
                if (!Directory.Exists(directory))
                {
                    Log.LogWarning($"Skipping loading components from {directory} as it does not exist.");
                    continue;
                }

                Log.LogWarning($"Loading components from {directory}");
                LoadCompiledComponents(directory);
            }
        }

        public MultiResolutionImage GetIcon(Tuple<Guid, string> identifier)
        {
            if (icons.TryGetValue(identifier, out MultiResolutionImage icon))
                return icon;
            return null;
        }

        public IReadOnlyCollection<ComponentDescription> AllDescriptions => LookupDictionary.Values.Distinct().ToList();

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
                            description.Source = new ComponentDescriptionSource(file);
                            
                            foreach (var type in description.GetComponentTypes())
                                AddDescription(type, description);
                        }
                    }
                }
            }
        }

        private void LoadCompiledComponents(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.cdcom", SearchOption.TopDirectoryOnly))
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var binaryLoader = new BinaryDescriptionReader();
                    if (binaryLoader.Read(fs))
                    {
                        foreach (var description in binaryLoader.ComponentDescriptions)
                        {
                            description.Metadata.Location = ComponentDescriptionMetadata.LocationType.Installed;
                            description.Source = new ComponentDescriptionSource(file);

                            foreach (var type in description.GetComponentTypes())
                                AddDescription(type, description);

                            if (description.Metadata.Icon is MultiResolutionImage icon)
                                icons.Add(Tuple.Create(Guid.Parse(description.ID), (string)null), icon);
                        }
                    }
                }
            }
        }
    }
}
