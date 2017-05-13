// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Render;
using CircuitDiagram.Render.Path;
using Point = CircuitDiagram.Primitives.Point;
using Size = CircuitDiagram.Primitives.Size;
using TextAlignment = CircuitDiagram.Drawing.Text.TextAlignment;

namespace CircuitDiagram.View.Editor
{
    class DrawingRenderer : IDrawingContext
    {
        public bool Absolute { get { return false; } }

        public DrawingContext Context { get; private set; }
        private Pen Pen { get; set; }

        public DrawingRenderer(DrawingContext context)
        {
            Context = context;
            Begin();
        }

        void Begin()
        {
            Pen = new Pen(Brushes.Black, 2d);
            Pen.StartLineCap = PenLineCap.Square;
            Pen.EndLineCap = PenLineCap.Square;
        }

        public void StartSection(object tag)
        {
            // Do nothing
        }

        public void DrawLine(Point start, Point end, double thickness)
        {
            Pen newPen = new Pen(Brushes.Black, thickness);
            newPen.StartLineCap = PenLineCap.Square;
            newPen.EndLineCap = PenLineCap.Square;
            Context.DrawLine(newPen, start.ToWinPoint(), end.ToWinPoint());
        }

        public void DrawRectangle(Point start, Size size, double thickness, bool fill = false)
        {
            Pen.Thickness = thickness;
            Context.DrawRectangle(fill ? Brushes.Black : null, Pen, new Rect(start.ToWinPoint(), size.ToWinSize()));
        }

        public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            Pen.Thickness = thickness;
            Context.DrawEllipse(fill ? Brushes.Black : null, Pen, centre.ToWinPoint(), radiusX, radiusY);
        }

        public void DrawPath(Point start, IList<IPathCommand> commands, double thickness, bool fill = false)
        {
            Pen newPen = new System.Windows.Media.Pen(Brushes.Black, thickness);
            newPen.StartLineCap = PenLineCap.Square;
            newPen.EndLineCap = PenLineCap.Square;
            Context.DrawGeometry((fill ? Brushes.Black : null), newPen, RenderHelper.GetGeometry(start.ToWinPoint(), commands, fill));
        }

        public void DrawText(Point anchor, Drawing.Text.TextAlignment alignment, IList<TextRun> textRuns)
        {
            double totalWidth = 0d;
            double totalHeight = 0d;

            foreach (TextRun run in textRuns)
            {
                FormattedText ft = new FormattedText(run.Text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(RenderHelper.CircuitFont()), run.Formatting.Size, Brushes.Black);
                if (run.Formatting.FormattingType == TextRunFormattingType.Subscript)
                    ft = new FormattedText(run.Text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(RenderHelper.CircuitFont()), run.Formatting.Size / 1.5, Brushes.Black);
                totalWidth += ft.Width;
                if (ft.Height > totalHeight)
                    totalHeight = ft.Height;
            }

            var renderLocation = anchor.ToWinPoint();
            if (alignment == TextAlignment.TopCentre || alignment == TextAlignment.CentreCentre || alignment == TextAlignment.BottomCentre)
                renderLocation.X -= totalWidth / 2;
            else if (alignment == TextAlignment.TopRight || alignment == TextAlignment.CentreRight || alignment == TextAlignment.BottomRight)
                renderLocation.X -= totalWidth;
            if (alignment == TextAlignment.CentreLeft || alignment == TextAlignment.CentreCentre || alignment == TextAlignment.CentreRight)
                renderLocation.Y -= totalHeight / 2;
            else if (alignment == TextAlignment.BottomLeft || alignment == TextAlignment.BottomCentre || alignment == TextAlignment.BottomRight)
                renderLocation.Y -= totalHeight;

            double horizontalOffsetCounter = 0;
            foreach (TextRun run in textRuns)
            {
                if (run.Formatting.FormattingType == TextRunFormattingType.Normal)
                {
                    FormattedText formattedText = new FormattedText(run.Text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(RenderHelper.CircuitFont()), run.Formatting.Size, Brushes.Black);
                    Context.DrawText(formattedText, System.Windows.Point.Add(renderLocation, new Vector(horizontalOffsetCounter, 0d)));
                    horizontalOffsetCounter += formattedText.Width;
                }
                else if (run.Formatting.FormattingType == TextRunFormattingType.Subscript)
                {
                    FormattedText formattedText = new FormattedText(run.Text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(RenderHelper.CircuitFont()), run.Formatting.Size / 1.5, Brushes.Black);
                    Context.DrawText(formattedText, System.Windows.Point.Add(renderLocation, new Vector(horizontalOffsetCounter, totalHeight - formattedText.Height)));
                    horizontalOffsetCounter += formattedText.Width;
                }
                else if (run.Formatting.FormattingType == TextRunFormattingType.Superscript)
                {
                    FormattedText formattedText = new FormattedText(run.Text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(RenderHelper.CircuitFont()), run.Formatting.Size / 1.5, Brushes.Black);
                    Context.DrawText(formattedText, System.Windows.Point.Add(renderLocation, new Vector(horizontalOffsetCounter, -3d)));
                    horizontalOffsetCounter += formattedText.Width;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
