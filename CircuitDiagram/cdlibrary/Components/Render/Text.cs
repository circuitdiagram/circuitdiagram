// Text.cs
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
    class Text : IRenderCommand
    {
        public ComponentPoint Location { get; set; }
        public TextAlignment Alignment { get; set; }
        public double Size { get; set; }
        public string Value { get; set; }

        public RenderCommandType Type
        {
            get { return RenderCommandType.Text; }
        }

        public Text()
        {
            Location = new ComponentPoint();
            Alignment = TextAlignment.TopLeft;
            Size = 12d;
            Value = "";
        }

        public Text(ComponentPoint location, TextAlignment alignment, double size, string value)
        {
            Location = location;
            Alignment = alignment;
            Size = size;
            Value = value;
        }

        public void Render(Component component, DrawingContext dc, Color colour)
        {
            // Resolve value
            string renderValue;
            if (Value.StartsWith("$"))
            {
                ComponentProperty property = component.FindProperty(Value);
                renderValue = component.GetFormattedProperty(property);
            }
            else
                renderValue = Value;

            FormattedText formattedText = new FormattedText(renderValue, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), Size, new SolidColorBrush(colour));
            Point renderLocation = Location.Resolve(component);

            if (Alignment == TextAlignment.TopCentre || Alignment == TextAlignment.CentreCentre || Alignment == TextAlignment.BottomCentre)
                renderLocation.X -= formattedText.Width / 2;
            else if (Alignment == TextAlignment.TopRight || Alignment == TextAlignment.CentreRight || Alignment == TextAlignment.BottomRight)
                renderLocation.X -= formattedText.Width;
            if (Alignment == TextAlignment.CentreLeft || Alignment == TextAlignment.CentreCentre || Alignment == TextAlignment.CentreRight)
                renderLocation.Y -= formattedText.Height / 2;
            else if (Alignment == TextAlignment.BottomLeft || Alignment == TextAlignment.BottomCentre || Alignment == TextAlignment.BottomRight)
                renderLocation.Y -= formattedText.Height;

            dc.DrawText(formattedText, renderLocation);
        }

        public void Render(Component component, CircuitDiagram.Render.IRenderContext dc)
        {
            // Resolve value
            string renderValue;
            if (Value.StartsWith("$"))
            {
                ComponentProperty property = component.FindProperty(Value);
                renderValue = component.GetFormattedProperty(property);
            }
            else
                renderValue = Value;

            dc.DrawText(Point.Add(Location.Resolve(component), new Vector(component.Offset.X, component.Offset.Y)), Alignment, renderValue, Size);
        }
    }

    public enum TextAlignment
    {
        TopLeft,
        TopCentre,
        TopRight,
        CentreLeft,
        CentreCentre,
        CentreRight,
        BottomLeft,
        BottomCentre,
        BottomRight
    }
}
