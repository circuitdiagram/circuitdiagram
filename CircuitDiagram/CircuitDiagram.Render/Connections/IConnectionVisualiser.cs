// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
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
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.Drawing;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Render.Connections
{
    /// <summary>
    /// Determines whether a connection should be shown when rendering a circuit.
    /// </summary>
    public interface IConnectionVisualiser
    {
        /// <summary>
        /// Recomputes connections in a circuit, and returns the points where connections should be rendered.
        /// </summary>
        IList<VisualisedConnection> PositionConnections(CircuitDocument document, LayoutOptions layoutOptions);
    }
}
