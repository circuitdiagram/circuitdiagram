// IOWire.cs
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
    /// Represents a wire within a stored document format.
    /// </summary>
    public class IOWire
    {
        /// <summary>
        /// The position of the wire within the document.
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        /// The size of the wire.
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// The orientation of the wire.
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Creates a new IOWire.
        /// </summary>
        public IOWire()
        {
        }

        /// <summary>
        /// Creates a new IOWire with the specified parameters.
        /// </summary>
        /// <param name="location">The position of the wire within the document.</param>
        /// <param name="size">The size of the wire.</param>
        /// <param name="orientation">The orientation of the wire.</param>
        public IOWire(Point location, double size, Orientation orientation)
        {
            Location = location;
            Size = size;
            Orientation = orientation;
        }
    }
}
