// MoveTo.cs
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
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Render.Path
{
    public class MoveTo : IPathCommand
    {
        public CommandType Type { get { return CommandType.MoveTo; } }
        public Point End { get { return new Point(X, Y); } }

        public double X { get; set; }
        public double Y { get; set; }

        public MoveTo()
        {
            X = 0;
            Y = 0;
        }

        public MoveTo(double x, double y)
        {
            X = x;
            Y = y;
        }

        public string Shorthand(Point offset, Point previous)
        {
            double x = X + offset.X;
            double y = Y + offset.Y;

            return "M " + x.ToString() + "," + y.ToString();
        }

        public IPathCommand Flip(bool horizontal)
        {
            if (horizontal)
            {
                return new MoveTo(-X, Y);
            }
            else
            {
                return new MoveTo(X, -Y);
            }
        }
    }
}
