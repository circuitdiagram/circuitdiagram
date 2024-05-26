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
using System.IO;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Render.Path;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Point = CircuitDiagram.Primitives.Point;
using Size = CircuitDiagram.Primitives.Size;
using TextAlignment = CircuitDiagram.Drawing.Text.TextAlignment;
using TextRun = CircuitDiagram.Drawing.Text.TextRun;

namespace CircuitDiagram.Render.ImageSharp
{
    public class ImageSharpDrawingContext : IPngDrawingContext
    {
        private static readonly SolidBrush Black = new SolidBrush(Color.Black);

        private readonly FontFamily fontFamily;
        private readonly Image<Argb32> image;
        private readonly bool ownsImage;

        public ImageSharpDrawingContext(int width, int height)
        : this(width, height, Color.Transparent)
        {
            fontFamily = SystemFonts.Get("Arial");
            image = new Image<Argb32>(width, height);
            ownsImage = true;
        }

        public ImageSharpDrawingContext(int width, int height, Color backgroundColor)
        {
            fontFamily = SystemFonts.Get("Arial");
            image = new Image<Argb32>(width, height);
            ownsImage = true;

            // Fill background
            image.Mutate(ctx => { ctx.Fill(backgroundColor, new RectangleF(0, 0, width, height)); });
        }

        public ImageSharpDrawingContext(Image<Argb32> target)
        {
            fontFamily = SystemFonts.Get("Arial");
            image = target;
            ownsImage = false;
        }

        public ImageSharpDrawingContext(Image<Argb32> target, FontFamily fontFamily)
        {
            this.fontFamily = fontFamily;
            image = target;
            ownsImage = false;
        }
        
        public void DrawLine(Point start, Point end, double thickness)
        {
            image.Mutate(ctx => ctx.DrawLine(Black, 2.0f, new[]
            {
                new PointF((float)start.X, (float)start.Y),
                new PointF((float)end.X, (float)end.Y)
            }));
        }

        public void DrawRectangle(Point start, Size size, double thickness, bool fill = false)
        {
            image.Mutate(ctx => ctx.DrawPolygon(Black, (float)thickness, new[]
            {
                new PointF((float)start.X, (float)start.Y),
                new PointF((float)start.X + (float)size.Width, (float)start.Y),
                new PointF((float)start.X + (float)size.Width, (float)start.Y + (float)size.Height),
                new PointF((float)start.X, (float)start.Y + (float)size.Height) 
            }));
        }

        public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            image.Mutate(ctx => ctx.Draw(Black, 2.0f, new EllipsePolygon(centre.ToPointF(), new SizeF((float)radiusX * 2, (float)radiusY * 2))));
        }

        public void DrawPath(Point start, IList<IPathCommand> commands, double thickness, bool fill = false)
        {
            var builder = new PathBuilder();
            builder.SetOrigin(new PointF((float)start.X, (float)start.Y));
            builder.StartFigure();
            PointF currentPoint = new PointF();
            foreach (var c in commands)
            {
                switch (c)
                {
                    case LineTo line:
                    {
                        builder.AddLine(currentPoint, line.End.ToPointF());
                        break;
                    }
                    case CurveTo curve:
                    {
                        builder.AddCubicBezier(currentPoint, curve.ControlStart.ToPointF(), curve.ControlEnd.ToPointF(), curve.End.ToPointF());
                        break;
                    }
                    case MoveTo move:
                    {
                        builder.StartFigure();
                        break;
                    }
                    case QuadraticBeizerCurveTo curve:
                    {
                        builder.AddQuadraticBezier(currentPoint, curve.Control.ToPointF(), curve.End.ToPointF());
                        break;
                    }
                    case EllipticalArcTo arc:
                    {
                        builder.AddArc(currentPoint, (float)arc.Size.Width, (float)arc.Size.Height, (float)arc.RotationAngle, arc.IsLargeArc, arc.SweepDirection == SweepDirection.Clockwise, arc.End.ToPointF());
                        break;
                    }
                    case ClosePath close:
                    {
                        builder.CloseFigure();
                        break;
                    }
                }
                currentPoint = c.End.ToPointF();
            }
            image.Mutate(ctx => ctx.Draw(Black, 2.0f, builder.Build()));
        }

        public void DrawText(Point anchor, TextAlignment alignment, double rotation, IList<TextRun> textRuns)
        {
            // TODO: support rotation

            var font = new Font(fontFamily, 12);

            image.Mutate(ctx =>
            {
                float totalWidth = 0f;
                float totalHeight = 0f;

                foreach (TextRun run in textRuns)
                {
                    Font renderFont = font;
                    if (run.Formatting.FormattingType == TextRunFormattingType.Subscript)
                        renderFont = new Font(font.Family, font.Size / 1.5f);
                    var dimensions = TextMeasurer.MeasureBounds(run.Text, new TextOptions(renderFont));
                    totalWidth += dimensions.Width;
                    totalHeight = Math.Max(totalHeight, dimensions.Bottom);
                }

                var startLocation = anchor.ToPointF();
                if (alignment == TextAlignment.TopCentre || alignment == TextAlignment.CentreCentre || alignment == TextAlignment.BottomCentre)
                    startLocation.X -= totalWidth / 2;
                else if (alignment == TextAlignment.TopRight || alignment == TextAlignment.CentreRight || alignment == TextAlignment.BottomRight)
                    startLocation.X -= totalWidth;
                if (alignment == TextAlignment.CentreLeft || alignment == TextAlignment.CentreCentre || alignment == TextAlignment.CentreRight)
                    startLocation.Y -= totalHeight / 2;
                else if (alignment == TextAlignment.BottomLeft || alignment == TextAlignment.BottomCentre || alignment == TextAlignment.BottomRight)
                    startLocation.Y -= totalHeight;

                float horizontalOffsetCounter = 0;
                foreach (TextRun run in textRuns)
                {
                    var renderFont = font;
                    var renderLocation = new PointF(startLocation.X + horizontalOffsetCounter, startLocation.Y);

                    if (run.Formatting.FormattingType == TextRunFormattingType.Subscript)
                    {
                        renderFont = new Font(font.Family, font.Size / 1.5f);
                        renderLocation.X = renderLocation.X + 3f;
                    }
                    else if (run.Formatting.FormattingType == TextRunFormattingType.Superscript)
                    {
                        renderFont = new Font(font.Family, font.Size / 1.5f);
                        renderLocation.X = renderLocation.X - 3f;
                    }

                    ctx.DrawText(run.Text, font, Color.Black, renderLocation);
                    horizontalOffsetCounter += TextMeasurer.MeasureBounds(run.Text, new TextOptions(renderFont)).Width;
                }
            });
        }

        public void WriteAsPng(Stream stream)
        {
            image.Save(stream, new PngEncoder());
        }

        public void Dispose()
        {
            if (ownsImage)
                image.Dispose();
        }
    }

    static class PointExtensions
    {
        public static PointF ToPointF(this Point point)
        {
            return new PointF((float)point.X, (float)point.Y);
        }
    }
}
