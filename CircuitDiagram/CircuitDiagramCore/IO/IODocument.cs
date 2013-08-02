// IODocument.cs
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
    /// Represents a circuit document as a stored document format.
    /// </summary>
    public class IODocument
    {
        /// <summary>
        /// Represents an unknown collection source.
        /// </summary>
        public const string UnknownCollection = "special:unknown";

        /// <summary>
        /// The document metadata.
        /// </summary>
        public DocumentMetadata Metadata { get; set; }

        /// <summary>
        /// The size of the document.
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// The components within the document.
        /// </summary>
        public List<IOComponent> Components { get; private set; }

        /// <summary>
        /// The wires within the document.
        /// </summary>
        public List<IOWire> Wires { get; private set; }

        /// <summary>
        /// Creates a new IODocument.
        /// </summary>
        public IODocument()
        {
            Metadata = new DocumentMetadata();
            Components = new List<IOComponent>();
            Wires = new List<IOWire>();
        }

        /// <summary>
        /// Creates a new IODocument with the specified parameters.
        /// </summary>
        /// <param name="size">The size of the document.</param>
        /// <param name="components">Components the document contains.</param>
        public IODocument(Size size, IEnumerable<IOComponent> components, IEnumerable<IOWire> wires)
        {
            Metadata = new DocumentMetadata();
            Size = size;
            Components = new List<IOComponent>(components);
            Wires = new List<IOWire>(wires);
        }

        /// <summary>
        /// Gets a dictionary containing all component types used in the document, grouped by collection.
        /// </summary>
        /// <returns>Dictionary of all component types used in document.</returns>
        public Dictionary<string, List<IOComponentType>> GetComponentTypes()
        {
            Dictionary<string, List<IOComponentType>> componentTypes = new Dictionary<string, List<IOComponentType>>();

            foreach (IOComponent component in Components)
            {
                string collection = component.Type.Collection;

                if (String.IsNullOrEmpty(component.Type.Collection))
                    collection = IODocument.UnknownCollection;

                if (!componentTypes.ContainsKey(collection))
                    componentTypes.Add(collection, new List<IOComponentType>());

                if (!componentTypes[collection].Contains(component.Type))
                    componentTypes[collection].Add(component.Type);
            }

            return componentTypes;
        }
    }
}
