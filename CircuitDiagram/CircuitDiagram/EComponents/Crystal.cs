// Crystal.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2011  Sam Fisher
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

namespace CircuitDiagram.EComponents
{
    class Crystal : EComponent
    {
        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(StartLocation.X, StartLocation.Y - 14d, Size.Width, 28d);
                else
                    return new Rect(StartLocation.X - 14d, StartLocation.Y, 28d, Size.Height);
            }
        }

        public override void Render(IRenderer dc, Color colour)
        {
            if (Horizontal)
            {
                Point start = new Point(StartLocation.X + Size.Width / 2 - 8d, StartLocation.Y);
                Point end = new Point(StartLocation.X + Size.Width / 2 + 8d, StartLocation.Y);
                dc.DrawLine(colour, 2d, StartLocation, start);
                dc.DrawLine(colour, 2d, end, EndLocation);
                dc.DrawLine(colour, 2d, new Point(start.X, start.Y - 10d), new Point(start.X, start.Y + 10d));
                dc.DrawLine(colour, 2d, new Point(end.X, end.Y - 10d), new Point(end.X, end.Y + 10d));
                dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(start.X + 4d, start.Y - 14d, 8d, 28d));
            }
            else
            {
                Point start = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 8d);
                Point end = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 8d);
                dc.DrawLine(colour, 2d, StartLocation, start);
                dc.DrawLine(colour, 2d, end, EndLocation);
                dc.DrawLine(colour, 2d, new Point(start.X - 10d, start.Y), new Point(start.X + 10d, start.Y));
                dc.DrawLine(colour, 2d, new Point(end.X - 10d, end.Y), new Point(end.X + 10d, end.Y));
                dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(start.X - 14d, start.Y + 4d, 28d, 8d));
            }
        }
    }
}
