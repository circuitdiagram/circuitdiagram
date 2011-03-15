// Counter.cs
//
// Circuit Diagram http://circuitdiagram.codeplex.com/
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
    public class Counter : EComponent
    {
        const double RectStart = 16d;

        public override Rect BoundingBox
        {
            get
            {
                return new Rect(RenderStartLocation.X, RenderStartLocation.Y -25f, 141f, 75f);
            }
        }

        public Counter()
        {
            CanResize = false;
        }

        public override void Render(IRenderer dc, Color color)
        {
            dc.DrawLine(color, 2.0f, new Point(RenderStartLocation.X, RenderStartLocation.Y), new Point(StartLocation.X + 9f, StartLocation.Y));
            dc.DrawRectangle(Colors.Transparent, color, 2f, new Rect(StartLocation.X + RectStart, StartLocation.Y - 25f, 120f, 50f));
            dc.DrawEllipse(Colors.Transparent, color, 2f, new Point(StartLocation.X + 11f, StartLocation.Y), 3d, 3d);
            dc.DrawPath(Colors.Transparent, color, 1f, String.Format("M {0} l 6,6 l -6,6", new Point(StartLocation.X + RectStart, StartLocation.Y - 6f).ToString()));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 14f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 14f, StartLocation.Y + 50f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 14f + 30f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 14f + 30f, StartLocation.Y + 50f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 14f + 60f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 14f + 60f, StartLocation.Y + 50f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 14f + 90f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 14f + 90f, StartLocation.Y + 50f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 120f, StartLocation.Y), new Point(StartLocation.X + 141f, StartLocation.Y));
            dc.DrawText("A", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f, StartLocation.Y + 10f));
            dc.DrawText("B", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 30f, StartLocation.Y + 10f));
            dc.DrawText("C", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 60f, StartLocation.Y + 10f));
            dc.DrawText("D", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 90f, StartLocation.Y + 10f));
            dc.DrawText("R", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 108f, StartLocation.Y - 6f));
        }
    }
}
