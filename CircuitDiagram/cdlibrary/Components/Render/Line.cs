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
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram.Components.Render
{
    public class Line : IRenderCommand
    {
        public ComponentPoint Start { get; set; }
        public ComponentPoint End { get; set; }
        public double Thickness { get; set; }

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

        public RenderCommandType Type
        {
            get { return RenderCommandType.Line; }
        }

        public void Render(Component component, CircuitDiagram.Render.IRenderContext dc)
        {
            if (dc.Absolute)
                dc.DrawLine(Point.Add(Start.Resolve(component), new Vector(component.Offset.X, component.Offset.Y)), Point.Add(End.Resolve(component), new Vector(component.Offset.X, component.Offset.Y)), Thickness);
            else
                dc.DrawLine(Start.Resolve(component), End.Resolve(component), Thickness);
        }
    }
}
