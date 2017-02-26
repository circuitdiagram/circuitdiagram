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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.View.Services
{
    class ConfigurationValues : IConfigurationValues
    {
        public ConfigurationValues()
        {
            string executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
#if DEBUG
            string configurationDirectory = Path.GetFullPath(Path.Combine(executingDirectory, "../../../../Config/AppData"));

            ComponentsDirectories = new[]
            {
                Path.Combine(executingDirectory, "../../../../", "Components/Output")
            };
#else
            string configurationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Circuit Diagram");
            
            ComponentsDirectories = new[]
            {
                Path.Combine(configurationDirectory, "components"),
                Path.Combine(executingDirectory, "ext")
            };
#endif

            PluginDirectories = new[]
            {
                Path.Combine(executingDirectory, "Plugins")
            };

            ToolboxConfigurationFile = Path.Combine(configurationDirectory, "toolbox.xml");
        }

        public string ToolboxConfigurationFile { get; }

        public string[] ComponentsDirectories { get; }

        public string[] PluginDirectories { get; }
    }
}
