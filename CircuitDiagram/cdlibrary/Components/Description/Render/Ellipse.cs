// Ellipse.cs
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

namespace CircuitDiagram.Components.Description.Render
{
    public class Ellipse : IRenderCommand
    {
        public ComponentPoint Centre { get; set; }
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }
        public double Thickness { get; set; }
        public bool Fill { get; set; }

        public RenderCommandType Type
        {
            get { return RenderCommandType.Ellipse; }
        }

        public Ellipse()
        {
            Centre = new ComponentPoint();
            RadiusX = 2d;
            RadiusY = 2d;
            Thickness = 2d;
            Fill = false;
        }

        public Ellipse(ComponentPoint centre, double radiusX, double radiusY, double thickness, bool fill)
        {
            Centre = centre;
            RadiusX = radiusX;
            RadiusY = radiusY;
            Thickness = thickness;
            Fill = fill;
        }

        public void Render(Component component, CircuitDiagram.Render.IRenderContext dc, bool absolute)
        {
            if (absolute)
                dc.DrawEllipse(Point.Add(Centre.Resolve(component), component.Location), RadiusX, RadiusY, Thickness, Fill);
            else
                dc.DrawEllipse(Centre.Resolve(component), RadiusX, RadiusY, Thickness, Fill);
        }
    }
}
