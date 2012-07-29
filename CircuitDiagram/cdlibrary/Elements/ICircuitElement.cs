// ICircuitElement.cs
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
using System.Windows.Media;
using CircuitDiagram.Render;

namespace CircuitDiagram.Elements
{
    /// <summary>
    /// Represents a visible item within a CircuitDocument.
    /// </summary>
    public interface ICircuitElement
    {
        /// <summary>
        /// The position of the element within the document.
        /// </summary>
        System.Windows.Vector Location { get; }

        /// <summary>
        /// Occurs when the element is updated.
        /// </summary>
        event EventHandler Updated;

        /// <summary>
        /// Renders the element using the specified IRenderContext.
        /// </summary>
        /// <param name="dc">The IRenderContext to use for rendering.</param>
        /// <param name="absolute">Whether to take the element's position within the document into account.</param>
        void Render(IRenderContext dc, bool absolute = true);
    }
}
