// This file is part of Circuit Diagram.
// Copyright (c) 2017 Samuel Fisher
//  
// Circuit Diagram is free software; you can redistribute it and/or
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
// along with Circuit Diagram. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render.Path;
using SixLabors.Fonts;
using Size = CircuitDiagram.Primitives.Size;
using TextAlignment = CircuitDiagram.Drawing.Text.TextAlignment;

namespace CircuitDiagram.Render.Drawing
{
    public class BufferedDrawingContext : IDrawingContext
    {
        private readonly TextMeasurer textMeasurer;
        private readonly IDrawingContext underlying;

        public BufferedDrawingContext()
            : this(null)
        {
        }

        public BufferedDrawingContext(IDrawingContext underlying)
        {
            textMeasurer = new TextMeasurer();
            this.underlying = underlying;
        }

        public Rect BoundingBox { get; private set; }

        public void DrawLine(Point start, Point end, double thickness)
        {
            Expand(new Rect(start, end));
            underlying?.DrawLine(start, end, thickness);
        }

        public void DrawRectangle(Point start, Size size, double thickness, bool fill = false)
        {
            Expand(new Rect(start, size));
            underlying?.DrawRectangle(start, size, thickness, fill);
        }

        public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            Expand(new Rect(centre.X - radiusX, centre.Y - radiusY, 2 * radiusX, 2 * radiusY));
            underlying?.DrawEllipse(centre, radiusX, radiusY, thickness, fill);
        }

        public void DrawPath(Point start, IList<IPathCommand> commands, double thickness, bool fill = false)
        {
            // Not supported
            underlying?.DrawPath(start, commands, thickness, fill);
        }

        public void DrawText(Point anchor, TextAlignment alignment, IList<TextRun> textRuns)
        {
            double totalWidth = 0.0;
            double height = 0.0;
            foreach (var textRun in textRuns)
            {
                var size = MeasureText(textRun);
                totalWidth += size.Width;
                height = Math.Max(height, size.Height);
            }

            double topLeftX;
            switch (alignment)
            {
                case TextAlignment.TopLeft:
                case TextAlignment.CentreLeft:
                case TextAlignment.BottomLeft:
                    topLeftX = anchor.X;
                    break;
                case TextAlignment.TopCentre:
                case TextAlignment.CentreCentre:
                case TextAlignment.BottomCentre:
                    topLeftX = anchor.X - totalWidth / 2;
                    break;
                case TextAlignment.TopRight:
                case TextAlignment.CentreRight:
                case TextAlignment.BottomRight:
                    topLeftX = anchor.X - totalWidth;
                    break;
                default:
                    throw new InvalidOperationException($"Missing case for {alignment}");
            }

            double topLeftY;
            switch (alignment)
            {
                case TextAlignment.TopLeft:
                case TextAlignment.TopCentre:
                case TextAlignment.TopRight:
                    topLeftY = anchor.Y;
                    break;
                case TextAlignment.CentreLeft:
                case TextAlignment.CentreCentre:
                case TextAlignment.CentreRight:
                    topLeftY = anchor.Y - height / 2;
                    break;
                case TextAlignment.BottomLeft:
                case TextAlignment.BottomCentre:
                case TextAlignment.BottomRight:
                    topLeftY = anchor.Y - height;
                    break;
                default:
                    throw new InvalidOperationException($"Missing case for {alignment}");
            }

            var topLeft = new Point(topLeftX, topLeftY);
            Expand(new Rect(topLeft, new Size(totalWidth, height)));

            underlying?.DrawText(anchor, alignment, textRuns);
            underlying?.DrawRectangle(topLeft, new Size(totalWidth, height), 1.0);
        }

        private Size MeasureText(TextRun text)
        {
            if (string.IsNullOrWhiteSpace(text.Text))
                return new Size(0, 0);

            // TODO: Support text.Formatting.FormattingType
            var family = FontCollection.SystemFonts.Find("Arial");
            var font = new Font(family, (float)text.Formatting.Size, FontStyle.Regular);
            var size = textMeasurer.MeasureText(text.Text, font, 72);
            return new Size(size.Width, size.Height);
        }

        private void Expand(Rect rect)
        {
            if (BoundingBox == null)
            {
                BoundingBox = rect;
                return;
            }

            BoundingBox = BoundingBox.Union(rect);
        }

        public void Dispose()
        {
            underlying?.Dispose();
        }
    }
}
