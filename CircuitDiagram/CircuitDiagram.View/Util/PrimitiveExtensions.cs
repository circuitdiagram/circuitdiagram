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
using System.Linq;
using System.Text;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Render
{
    static class PrimitiveExtensions
    {
        public static System.Windows.Point ToWinPoint(this Point point)
        {
            return new System.Windows.Point(point.X, point.Y);
        }

        public static System.Windows.Size ToWinSize(this Size size)
        {
            return new System.Windows.Size(size.Width, size.Height);
        }

        public static System.Windows.Vector ToWinVector(this Point point)
        {
            return new System.Windows.Vector(point.X, point.Y);
        }

        public static System.Windows.Vector ToWinVector(this Vector vector)
        {
            return new System.Windows.Vector(vector.X, vector.Y);
        }

        public static Point ToCdPoint(this System.Windows.Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static Point ToCdPoint(this System.Windows.Vector vector)
        {
            return new Point(vector.X, vector.Y);
        }

        public static Vector ToCdVector(this System.Windows.Vector vector)
        {
            return new Vector(vector.X, vector.Y);
        }

        public static Point SnapToGrid(this Point point, double gridSize)
        {
            return new Point(Math.Round(point.X / gridSize) * gridSize, Math.Round(point.Y / gridSize) * gridSize);
        }
    }
}
