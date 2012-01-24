// Rectangle.cs
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

namespace CircuitDiagram.Components.Render
{
    class Rectangle : IRenderCommand
    {
        public ComponentPoint Location { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double StrokeThickness { get; set; }
        public Color FillColour { get; set; }

        public RenderCommandType Type
        {
            get { return RenderCommandType.Rect; }
        }

        public Rectangle()
        {
            Location = new ComponentPoint();
            Width = 0d;
            Height = 0d;
            StrokeThickness = 2d;
            FillColour = Colors.Transparent;
        }

        public Rectangle(ComponentPoint location, double width, double height)
        {
            Location = location;
            Width = width;
            Height = height;
            StrokeThickness = 2d;
            FillColour = Colors.Transparent;
        }

        public Rectangle(ComponentPoint location, double width, double height, double strokeThickness, Color fillColour)
        {
            Location = location;
            Width = width;
            Height = height;
            StrokeThickness = strokeThickness;
            FillColour = fillColour;
        }

        public void Render(Component component, DrawingContext dc, Color colour)
        {
            dc.DrawRectangle(new SolidColorBrush(FillColour), new Pen(new SolidColorBrush(colour), 2d), new System.Windows.Rect(Location.Resolve(component), new Size(Width, Height)));
        }

        public void Render(Component component, CircuitDiagram.Render.IRenderContext dc)
        {
            dc.DrawRectangle(Point.Add(Location.Resolve(component), new Vector(component.Offset.X, component.Offset.Y)), new Size(Width, Height), StrokeThickness, (FillColour != Colors.White && FillColour != Colors.Transparent));
        }
    }
}
