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
using System.Windows.Input;
using System.Xml.Linq;
using CircuitDiagram.Circuit;
using CircuitDiagram.Render;
using CircuitDiagram.View.Services;

namespace CircuitDiagram.View.ToolboxView
{
    class XmlToolboxReader : IToolboxReader
    {
        private readonly IComponentIconProvider iconProvider;

        public XmlToolboxReader(IComponentIconProvider iconProvider)
        {
            this.iconProvider = iconProvider;
        }

        public ToolboxEntry[][] GetToolbox(Stream input, IReadOnlyCollection<ComponentType> availableTypes)
        {
            var doc = XDocument.Load(input);

            var entries = new List<ToolboxEntry[]>();
            foreach (var category in doc.Root.Elements("category"))
            {
                var categoryItems = new List<ToolboxEntry>();

                foreach (var item in category.Elements("component"))
                {
                    string id = item.Attribute("guid")?.Value;
                    if (id == null)
                        continue;

                    Guid guid;
                    if (!Guid.TryParse(id, out guid))
                        continue;

                    string configurationName = item.Attribute("configuration")?.Value;
                    string keyName = item.Attribute("key")?.Value;
                    Key? key = null;
                    Key keyValue;
                    if (Enum.TryParse(keyName, true, out keyValue))
                        key = keyValue;

                    var type = availableTypes.FirstOrDefault(x => x.Id == guid);
                    if (type == null)
                        continue;

                    var configuration = type.Configurations.FirstOrDefault(x => x.Name == configurationName);

                    var entry = new ToolboxEntry
                    {
                        Name = configuration?.Name ?? type.Name.Value,
                        Type = type,
                        Configuration = configuration,
                        Key = key
                    };
                    entry.Icon = iconProvider.GetIcon(entry);

                    categoryItems.Add(entry);
                }

                entries.Add(categoryItems.ToArray());
            }

            return entries.ToArray();
        }
    }
}
