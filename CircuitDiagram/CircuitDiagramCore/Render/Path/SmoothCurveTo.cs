// SmoothCurveTo.cs
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
using System.Windows.Media;

namespace CircuitDiagram.Render.Path
{
    public class SmoothCurveTo : IPathCommand
    {
        public Point ControlStart { get; set; }
        public Point ControlEnd { get; set; }
        public Point End { get; set; }

        public CommandType Type
        {
            get { return CommandType.SmoothCurveTo; }
        }

        public SmoothCurveTo()
        {
            ControlStart = new Point();
            ControlEnd = new Point();
            End = new Point();
        }

        public SmoothCurveTo(double x1, double y1, double x2, double y2, double x, double y)
        {
            ControlStart = new Point(x1, y1);
            ControlEnd = new Point(x2, y2);
            End = new Point(x, y);
        }

        public string Shorthand(Point offset, Point previous)
        {
            return String.Format("c {0},{1} {2},{3} {4},{5}", ControlStart.X, ControlStart.Y, ControlEnd.X, ControlEnd.Y, End.X, End.Y);
        }

        public IPathCommand Flip(bool horizontal)
        {
            if (horizontal)
            {
                return new SmoothQuadraticBeizerCurveTo(-ControlStart.X, ControlStart.Y, -ControlEnd.X, ControlEnd.Y, -End.X, End.Y);
            }
            else
            {
                return new SmoothQuadraticBeizerCurveTo(ControlStart.X, -ControlStart.Y, ControlEnd.X, -ControlEnd.Y, End.X, -End.Y);
            }
        }
    }
}
