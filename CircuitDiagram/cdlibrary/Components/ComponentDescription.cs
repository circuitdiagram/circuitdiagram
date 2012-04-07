// ComponentDescription.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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
using System.Linq;
using System.Text;
using CircuitDiagram.Components.Render;

namespace CircuitDiagram.Components
{
    public class ComponentDescription
    {
        /// <summary>
        /// A unique ID for this description during runtime.
        /// </summary>
        public int RuntimeID { get; set; }

        public string ID { get; set; }
        public string ComponentName { get; private set; }
        public bool CanResize { get; private set; }
        public bool CanFlip { get; private set; }
        public double MinSize { get; private set; }
        public ComponentProperty[] Properties { get; private set; }
        public ConnectionGroup[] Connections { get; private set; }
        public RenderDescription[] RenderDescriptions { get; private set; }
        public Conditional<FlagOptions>[] Flags { get; private set; }
        public ComponentDescriptionMetadata Metadata { get; private set; }
        public ComponentDescriptionSource Source { get; set; }

        public ComponentDescription(string id, string componentName, bool canResize, bool canFlip, double minSize, ComponentProperty[] properties, ConnectionGroup[] connections, RenderDescription[] renderDescriptions, Conditional<FlagOptions>[] flags, ComponentDescriptionMetadata metadata)
        {
            ID = id;
            ComponentName = componentName;
            CanResize = canResize;
            CanFlip = canFlip;
            MinSize = minSize;
            Properties = properties;
            Connections = connections;
            RenderDescriptions = renderDescriptions;
            Flags = flags;
            Metadata = metadata;
        }
    }
}