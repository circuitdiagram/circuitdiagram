// Line.cs
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
using CircuitDiagram.Circuit;
using CircuitDiagram.Drawing;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.Drawing
{
    public class Line : IRenderCommand
    {
        public ComponentPoint Start { get; set; }
        public ComponentPoint End { get; set; }
        public double Thickness { get; set; }
        public RenderCommandType Type => RenderCommandType.Line;

        public Line()
        {
            Start = new ComponentPoint();
            End = new ComponentPoint();
            Thickness = 2d;
        }

        public Line(ComponentPoint start, ComponentPoint end, double thickness)
        {
            Start = start;
            End = end;
            Thickness = thickness;
        }
        
        public void Render(LayoutInformation layout, ILayoutContext layoutContext, IDrawingContext drawingContext)
        {
            var start = Start.Resolve(layout, layoutContext.Options);
            var end = End.Resolve(layout, layoutContext.Options);

            if (layoutContext.Options.Absolute)
                drawingContext.DrawLine(Point.Add(start, layout.Location), Point.Add(end, layout.Location), Thickness);
            else
                drawingContext.DrawLine(start, end, Thickness);
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Line return false.
            Line o = obj as Line;
            if ((System.Object)o == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Start.Equals(o.Start)
                && End.Equals(o.End)
                && Thickness.Equals(o.Thickness));
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode()
                ^ End.GetHashCode()
                ^ Thickness.GetHashCode();
        }
    }
}
