// IOComponent.cs
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
using System.Windows;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Represents a component within a stored document format.
    /// </summary>
    public class IOComponent
    {
        /// <summary>
        /// Unique identifier for this component within the document.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The position of the component within the document. If null, the location is not available.
        /// </summary>
        public Point? Location { get; set; }

        /// <summary>
        /// The size of the component. If null, the component cannot be resized.
        /// </summary>
        public double? Size { get; set; }

        /// <summary>
        /// Whether the component is flipped. If null, the component cannot be flipped.
        /// </summary>
        public bool? IsFlipped { get; set; }

        /// <summary>
        /// The orientation of the component. If null, the orientatino of the component cannot be changed.
        /// </summary>
        public Orientation? Orientation { get; set; }

        /// <summary>
        /// The type of component.
        /// </summary>
        public IOComponentType Type { get; set; }

        /// <summary>
        /// Additional properties for the component.
        /// </summary>
        public List<IOComponentProperty> Properties { get; private set; }

        /// <summary>
        /// Connections for the component, in the format Name-ConnectionID.
        /// </summary>
        public Dictionary<string, string> Connections { get; private set; }

        /// <summary>
        /// Initializes a new, empty IOComponent.
        /// </summary>
        public IOComponent()
        {
            Properties = new List<IOComponentProperty>();
            Connections = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new IOComponent with the specified parameters.
        /// </summary>
        /// <param name="location">The position of the component within the document.</param>
        /// <param name="size">The size of the component.</param>
        /// <param name="isFlipped">Whether the component is flipped.</param>
        /// <param name="orientation">The orientation of the component.</param>
        /// <param name="type">The type of component.</param>
        public IOComponent(string id, Point? location, double? size, bool? isFlipped, Orientation? orientation, IOComponentType type)
        {
            ID = id;
            Location = location;
            Size = size;
            IsFlipped = isFlipped;
            Orientation = orientation;
            Type = type;
            Properties = new List<IOComponentProperty>();
            Connections = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new IOComponent with the specified parameters.
        /// </summary>
        /// <param name="location">The position of the component within the document.</param>
        /// <param name="size">The size of the component.</param>
        /// <param name="isFlipped">Whether the component is flipped.</param>
        /// <param name="orientation">The orientation of the component.</param>
        /// <param name="type">The type of component.</param>
        /// <param name="properties">Additional properties for the component.</param>
        /// <param name="connections">Connections for the component.</param>
        public IOComponent(string id, Point? location, double? size, bool? isFlipped, Orientation? orientation, IOComponentType type, IEnumerable<IOComponentProperty> properties, IDictionary<string, string> connections)
        {
            ID = id;
            Location = location;
            Size = size;
            IsFlipped = isFlipped;
            Orientation = orientation;
            Type = type;
            Properties = new List<IOComponentProperty>(properties);
            Connections = new Dictionary<string, string>(connections);
        }
    }
}
