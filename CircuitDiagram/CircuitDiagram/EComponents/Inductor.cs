// Inductor.cs
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
using System.Globalization;

namespace CircuitDiagram.EComponents
{
    class Inductor : EComponent
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
                    return new Rect(StartLocation.X, StartLocation.Y - 10d, Size.Width, 14d);
                else
                    return new Rect(StartLocation.X - 4d, StartLocation.Y, 14d, Size.Height);
            }
        }

        public override void UpdateLayout()
        {
            this.ImplementMinimumSize(60d);
        }

        public override void Render(IRenderer dc, System.Windows.Media.Color colour)
        {
            if (Horizontal)
            {
                Point start = new Point(StartLocation.X + Size.Width / 2 - 24d, StartLocation.Y);
                Point end = new Point(StartLocation.X + Size.Width / 2 + 24d, StartLocation.Y);
                dc.DrawLine(colour, 2d, StartLocation, start);
                dc.DrawLine(colour, 2d, end, EndLocation);
                dc.DrawPath(null, colour, 2d, String.Format("M {0} a 2,1.5 90 0 1 12,0 m 0,0 a 2,1.5 90 0 1 12,0 m 0,0 a 2,1.5 90 0 1 12,0 m 0,0 a 2,1.5 90 0 1 12,0", start.ToString(CultureInfo.InvariantCulture)));
            }
            else
            {
                Point start = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 24d);
                Point end = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 24d);
                dc.DrawLine(colour, 2d, StartLocation, start);
                dc.DrawLine(colour, 2d, end, EndLocation);
                dc.DrawPath(null, colour, 2d, String.Format("M {0} a 1.5,2 90 0 1 0,12 m 0,0 a 1.5,2 90 0 1 0,12 m 0,0 a 1.5,2 90 0 1 0,12 m 0,0 a 1.5,2 90 0 1 0,12 m 0,0", start.ToString(CultureInfo.InvariantCulture)));
            }
        }
    }
}
