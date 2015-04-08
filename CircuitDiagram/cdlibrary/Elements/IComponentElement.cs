// IComponentElement.cs
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

using CircuitDiagram.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CircuitDiagram.Elements
{
    /// <summary>
    /// Represents a visible component within a circuit document.
    /// </summary>
    public interface IComponentElement : ICircuitElement
    {
        /// <summary>
        /// The collection this component belongs to.
        /// </summary>
        string ImplementationCollection { get; }

        /// <summary>
        /// The item within the collection this component belongs to.
        /// </summary>
        string ImplementationItem { get; }

        /// <summary>
        /// The size of the component.
        /// </summary>
        double Size { get; }

        /// <summary>
        /// Whether the component is flipped.
        /// </summary>
        bool IsFlipped { get; }

        /// <summary>
        /// Whether the component is horizontal.
        /// </summary>
        Orientation Orientation { get; }

        /// <summary>
        /// Properties of the component.
        /// </summary>
        IDictionary<string, PropertyUnion> Properties { get; }
    }
}
