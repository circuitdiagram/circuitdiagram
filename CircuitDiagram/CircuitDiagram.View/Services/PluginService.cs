// This file is part of Circuit Diagram.
// http://www.circuit-diagram.org/
// 
// Copyright (c) 2017 Samuel Fisher
//  
// Circuit Diagram is free software; you can redistribute it and/or
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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Plugin;

namespace CircuitDiagram.View.Services
{
    class PluginService : IPluginService
    {
        [ImportMany(typeof(IPlugin))] private IEnumerable<Lazy<IPlugin, IPluginMetadata>> plugins;

        private IEnumerable<Lazy<IPlugin, IPluginMetadata>> enabledPlugins => plugins;

        public PluginService()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(@"C:\Users\Sam\Documents\Projects\CircuitDiagram\CircuitDiagram\CircuitDiagram\bin\Debug\plugins"));

            var container = new CompositionContainer(catalog);

            try
            {
                container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }
        }

        public IReadOnlyList<IPluginMetadata> GetPlugins()
        {
            return plugins.Select(x => x.Metadata).ToList();
        }

        public IEnumerable<T> GetPluginParts<T>()
        {
            foreach (var plugin in enabledPlugins.SelectMany(p => p.Value.PluginParts.Where(x => x is T).Cast<T>()))
                yield return plugin;
        }
    }
}
