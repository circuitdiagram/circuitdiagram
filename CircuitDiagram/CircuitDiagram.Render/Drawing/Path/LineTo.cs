// LineTo.cs
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
    public class LineTo : IPathCommand
    {
        public CommandType Type { get { return CommandType.LineTo; } }
        public Point End { get { return new Point(X, Y); } }

        public double X { get; set; }
        public double Y { get; set; }

        public LineTo()
        {
            X = 0;
            Y = 0;
        }

        public LineTo(double x, double y)
        {
            X = x;
            Y = y;
        }

        public string Shorthand(Point offset, Point previous)
        {
            double x = X + offset.X;
            double y = Y + offset.Y;

            return "L " + x.ToString() + "," + y.ToString();
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
        }

        public void Read(System.IO.BinaryReader reader)
        {
            X = reader.ReadDouble();
            Y = reader.ReadDouble();
        }

        public IPathCommand Flip(bool horizontal)
        {
            if (horizontal)
            {
                return new LineTo(-X, Y);
            }
            else
            {
                return new LineTo(X, -Y);
            }
        }
    }
}
