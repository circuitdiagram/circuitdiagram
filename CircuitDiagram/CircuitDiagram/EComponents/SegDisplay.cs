// SegDisplay.cs
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
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram.EComponents
{
    public class SegDisplay : EComponent
    {
        public SegDisplay()
        {
        }

        public override void UpdateLayout()
        {
            if (EndLocation.Y - StartLocation.Y < 110f)
                EndLocation = new Point(EndLocation.X, StartLocation.Y + 110f);
        }

        public override Rect BoundingBox
        {
            get
            {
                return new Rect(RenderStartLocation.X - 41f, RenderStartLocation.Y, 75f, RenderEndLocation.Y - RenderStartLocation.Y);
            }
        }

        public override void Render(IRenderer dc, Color color)
        {
            dc.DrawPath(null, color, 2f, "M " + new Point(StartLocation.X - 41f, StartLocation.Y).ToString() + " l 75,0 l 0,110 l -75,0 l 0,-111 m 0,1 " + "m 10,10 l 55,0 l 0,90 l -55,0 l 0,-91 m 1,2 " + "l 14,14 l 25,0" +
                "l 14,-14 m -14,14 l 0,22.5 l 14,6.5 m 0,2 l -14,6.5 l 0,22.5 l 14,14 m -14,-14 l -25,0 l -14,14 m 14,-14 l 0,-22.5 l -14,-6.5 m 0,-2 l 14,-6.5 l 0,-22.5 m 0,22.5 l 25,0 m 0,15 l -25,0");
            dc.DrawLine(color, 2f, new Point(StartLocation.X, StartLocation.Y + 110), new Point(StartLocation.X, RenderEndLocation.Y));
        }
    }
}