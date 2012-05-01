// IPathCommand.cs
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
using System.Windows;
using System.IO;

namespace CircuitDiagram.Render.Path
{
    /// <summary>
    /// Represents a single element within a path.
    /// </summary>
    public interface IPathCommand
    {
        /// <summary>
        /// The end point for this path command.
        /// </summary>
        Point End { get; }

        /// <summary>
        /// Type of path command.
        /// </summary>
        CommandType Type { get; }

        /// <summary>
        /// Gets the SVG path syntax for the path command.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="previous"></param>
        /// <returns></returns>
        string Shorthand(Point offset, Point previous);

        /// <summary>
        /// Flips the path along the specified axis.
        /// </summary>
        /// <param name="horizontal">Whether to flip along the horizontal axis.</param>
        /// <returns>A flipped copy of the path command.</returns>
        IPathCommand Flip(bool horizontal);
    }
}
