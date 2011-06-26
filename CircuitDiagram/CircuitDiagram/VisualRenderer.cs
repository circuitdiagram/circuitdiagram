// VisualRenderer.cs
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
using System.Windows.Shapes;

namespace CircuitDiagram
{
    class VisualRenderer : IRenderer
    {
        public DrawingContext DrawingContext { get; set; }

        public void DrawLine(Color color, double thickness, Point point0, Point point1)
        {
            DrawingContext.DrawLine(new Pen(new SolidColorBrush(color), thickness), point0, point1);
        }

        public void DrawEllipse(Color fillColor, Color strokeColor, double strokeThickness, Point centre, double radiusX, double radiusY)
        {
            DrawingContext.DrawEllipse(new SolidColorBrush(fillColor), new Pen(new SolidColorBrush(strokeColor), strokeThickness), centre, radiusX, radiusY);
        }

        public void DrawRectangle(Color fillColor, Color strokeColor, double strokeThickness, Rect rectangle)
        {
            DrawingContext.DrawRectangle(new SolidColorBrush(fillColor), new Pen(new SolidColorBrush(strokeColor), strokeThickness), rectangle);
        }

        public void DrawText(string text, string fontName, double emSize, Color foreColor, Point origin)
        {
            DrawingContext.DrawText(new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(fontName), emSize, new SolidColorBrush(foreColor)), origin);
        }

        public void DrawFormattedText(FormattedText text, Point origin)
        {
            DrawingContext.DrawText(text, origin);
        }

        public void DrawPath(Color? fillColor, Color strokeColor, double thickness, string path)
        {
            Geometry geometry = Geometry.Parse(path);
            DrawingContext.DrawGeometry((fillColor.HasValue ? new SolidColorBrush(fillColor.Value) : null), new Pen(new SolidColorBrush(strokeColor), thickness), geometry);
        }
    }
}
