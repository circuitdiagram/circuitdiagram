// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
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
using System.Text;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render.Drawing;
using SkiaSharp;

namespace CircuitDiagram.Render.Skia
{
    public class SkiaBufferedDrawingContext : BufferedDrawingContext
    {
        protected override Size MeasureText(TextRun text)
        {
            if (string.IsNullOrEmpty(text.Text))
                return new Size();

            var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextSize = (float)text.Formatting.Size,
            };

            var bounds = new SKRect();
            paint.MeasureText(Encoding.UTF8.GetBytes(text.Text), ref bounds);

            return new Size(bounds.Width, bounds.Height);
        }
    }
}
