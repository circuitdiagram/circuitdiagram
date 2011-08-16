// SegDecoder.cs
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
    public class SegDecoder : EComponent
    {
        const double RectStart = 16d;

        public override Rect BoundingBox
        {
            get
            {
                return new Rect(RenderStartLocation.X + 16d, RenderStartLocation.Y - 25f, 120f, 50f);
            }
        }

        public SegDecoder()
        {
            CanResize = false;
        }

        public override void Render(IRenderer dc, Color color)
        {
            dc.DrawRectangle(Colors.Transparent, color, 2f, new Rect(RenderStartLocation.X + RectStart, RenderStartLocation.Y - 25f, 120f, 50f));

            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f, RenderStartLocation.Y - 25f), new Point(RenderStartLocation.X + RectStart + 15f, RenderStartLocation.Y - 50f));
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 30f, RenderStartLocation.Y - 25f), new Point(RenderStartLocation.X + RectStart + 15f + 30f, RenderStartLocation.Y - 50f));
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 60f, RenderStartLocation.Y - 25f), new Point(RenderStartLocation.X + RectStart + 15f + 60f, RenderStartLocation.Y - 50f));
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 90f, RenderStartLocation.Y - 25f), new Point(RenderStartLocation.X + RectStart + 15f + 90f, RenderStartLocation.Y - 50f));
            dc.DrawText("A", "Arial", 11d, color, new Point(RenderStartLocation.X + RectStart + 10f, RenderStartLocation.Y - 22f));
            dc.DrawText("B", "Arial", 11d, color, new Point(RenderStartLocation.X + RectStart + 10f + 30f, RenderStartLocation.Y - 22f));
            dc.DrawText("C", "Arial", 11d, color, new Point(RenderStartLocation.X + RectStart + 10f + 60f, RenderStartLocation.Y - 22f));
            dc.DrawText("D", "Arial", 11d, color, new Point(RenderStartLocation.X + RectStart + 10f + 90f, RenderStartLocation.Y - 22f));

            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f, RenderStartLocation.Y + 25f), new Point(RenderStartLocation.X + RectStart + 15f, RenderStartLocation.Y + 60f)); // a
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 15f, RenderStartLocation.Y + 25f), new Point(RenderStartLocation.X + RectStart + 15f + 15f, RenderStartLocation.Y + 50f)); // b
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 30f, RenderStartLocation.Y + 25f), new Point(RenderStartLocation.X + RectStart + 15f + 30f, RenderStartLocation.Y + 40f)); // c
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 45f, RenderStartLocation.Y + 25f), new Point(RenderStartLocation.X + RectStart + 15f + 45f, RenderStartLocation.Y + 70f)); // d
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 60f, RenderStartLocation.Y + 25f), new Point(RenderStartLocation.X + RectStart + 15f + 60f, RenderStartLocation.Y + 40f)); // e
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 75f, RenderStartLocation.Y + 25f), new Point(RenderStartLocation.X + RectStart + 15f + 75f, RenderStartLocation.Y + 50f)); // f
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 90f, RenderStartLocation.Y + 25f), new Point(RenderStartLocation.X + RectStart + 15f + 90f, RenderStartLocation.Y + 60f)); // g
            dc.DrawText("a", "Arial", 11d, color, new Point(RenderStartLocation.X + RectStart + 10f, RenderStartLocation.Y + 10f));
            dc.DrawText("b", "Arial", 11d, color, new Point(RenderStartLocation.X + RectStart + 10f + 15f, RenderStartLocation.Y + 10f));
            dc.DrawText("c", "Arial", 11d, color, new Point(RenderStartLocation.X + RectStart + 10f + 30f, RenderStartLocation.Y + 10f));
            dc.DrawText("d", "Arial", 11d, color, new Point(RenderStartLocation.X + RectStart + 10f + 45f, RenderStartLocation.Y + 10f));
            dc.DrawText("e", "Arial", 11d, color, new Point(RenderStartLocation.X + RectStart + 10f + 60f, RenderStartLocation.Y + 10f));
            dc.DrawText("f", "Arial", 11d, color, new Point(RenderStartLocation.X + RectStart + 10f + 75f, RenderStartLocation.Y + 10f));
            dc.DrawText("g", "Arial", 11d, color, new Point(RenderStartLocation.X + RectStart + 10f + 90f, RenderStartLocation.Y + 10f));
            // a
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f - 1f, RenderStartLocation.Y + 60f), new Point(RenderStartLocation.X + RectStart + 15f + 15f, RenderStartLocation.Y + 60f));
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 15f, RenderStartLocation.Y + 60f - 1f), new Point(RenderStartLocation.X + RectStart + 15f + 15f, RenderStartLocation.Y + 70f));
            // b
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 15f - 1f, RenderStartLocation.Y + 50f), new Point(RenderStartLocation.X + RectStart + 15f + 25f, RenderStartLocation.Y + 50f));
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 25f, RenderStartLocation.Y + 50f - 1f), new Point(RenderStartLocation.X + RectStart + 15f + 25f, RenderStartLocation.Y + 70f));
            // c
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 30f - 1f, RenderStartLocation.Y + 40f), new Point(RenderStartLocation.X + RectStart + 15f + 35f, RenderStartLocation.Y + 40f));
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 35f, RenderStartLocation.Y + 40f - 1f), new Point(RenderStartLocation.X + RectStart + 15f + 35f, RenderStartLocation.Y + 70f));
            // e
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 60f + 1f, RenderStartLocation.Y + 40f), new Point(RenderStartLocation.X + RectStart + 15f + 55f, RenderStartLocation.Y + 40f));
            dc.DrawLine(color, 2f, new Point(RenderStartLocation.X + RectStart + 15f + 55f, RenderStartLocation.Y + 40f - 1f), new Point(RenderStartLocation.X + RectStart + 15f + 55f, RenderStartLocation.Y + 70f));
            //f + g
            dc.DrawPath(null, color, 2f, "M " + new Point(RenderStartLocation.X + RectStart + 15f + 75f + 1f, RenderStartLocation.Y + 50f).ToString() + " l -11,0 m 1,0 l 0,20 m 25,-10 l -16,0 m 1,0 l 0,10");
        }
    }
}