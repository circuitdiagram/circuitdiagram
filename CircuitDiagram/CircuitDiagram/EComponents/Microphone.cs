// Microphone.cs
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
    class Microphone : EComponent
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
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 8d), new Point(EndLocation.X, EndLocation.Y + 24d));
                else
                    return new Rect(new Point(StartLocation.X - 24d, StartLocation.Y), new Point(EndLocation.X + 8d, EndLocation.Y));
            }
        }

        public override void UpdateLayout()
        {
            this.ImplementMinimumSize(30d);
        }

        public override void Render(IRenderer dc, Color colour)
        {
            if (Horizontal)
            {
                Point start = new Point(StartLocation.X + Size.Width / 2 - 12d, StartLocation.Y);
                Point end = new Point(StartLocation.X + Size.Width / 2 + 12d, StartLocation.Y);
                dc.DrawLine(colour, 2d, StartLocation, start);
                dc.DrawLine(colour, 2d, end, EndLocation);
                dc.DrawEllipse(Colors.Transparent, colour, 2d, new Point(StartLocation.X + Size.Width / 2, StartLocation.Y + 6d), 14d, 14d);
                dc.DrawRectangle(colour, colour, 2d, new Rect(start.X, StartLocation.Y + 20d, 24d, 4d));
            }
            else
            {
                Point start = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 12d);
                Point end = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 12d);
                dc.DrawLine(colour, 2d, StartLocation, start);
                dc.DrawLine(colour, 2d, end, EndLocation);
                dc.DrawEllipse(Colors.Transparent, colour, 2d, new Point(StartLocation.X - 6d, StartLocation.Y + Size.Height / 2), 14d, 14d);
                dc.DrawRectangle(colour, colour, 2d, new Rect(StartLocation.X - 24d, start.Y, 4d, 24d));
            }
        }
    }
}
