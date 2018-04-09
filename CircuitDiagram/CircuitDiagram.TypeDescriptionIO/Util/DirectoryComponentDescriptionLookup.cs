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
                            description.Source = new ComponentDescriptionSource(file);

                            foreach(var type in description.GetComponentTypes())
                                internalLookup.AddDescription(type, description);
                        }
                    }
                }
            }
        }
    }
}
