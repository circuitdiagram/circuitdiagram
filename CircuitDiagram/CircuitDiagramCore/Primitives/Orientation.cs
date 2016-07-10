// Orientation.cs
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

namespace CircuitDiagram.Primitives
{
    /// <summary>
    /// Represents either horizontal or vertical.
    /// </summary>
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    public static class OrientationExtensions
    {
        /// <summary>
        /// Reverses the orientation.
        /// </summary>
        /// <param name="orientation">The orientation to reverse.</param>
        /// <returns>The opposite orientation to that given.</returns>
        public static Orientation Reverse(this Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
                return Orientation.Vertical;
            else
                return Orientation.Horizontal;
        }
    }
}
