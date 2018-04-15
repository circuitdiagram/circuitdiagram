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
using System.IO;
using System.Text;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render.Path;
using SkiaSharp;

namespace CircuitDiagram.Render.Skia
{
    public class SkiaDrawingContext : IDrawingContext
    {
        private readonly SKSurface surface;

        public SkiaDrawingContext(int width, int height, SKColor background)
        {
            //var info = new SKImageInfo(width, height);
            surface = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Opaque);
            surface.Canvas.Clear(background);
        }

        public SKColor Color { get; set; } = SKColors.Black;

        public void Dispose()
        {
            surface.Dispose();
        }

        public void DrawLine(Point start, Point end, double thickness)
        {
            var paint = new SKPaint
            {
                Color = Color,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = (float)thickness,
                StrokeCap = SKStrokeCap.Square,
            };

            surface.Canvas.DrawLine(start.ToSkPoint(), end.ToSkPoint(), paint);
        }

        public void DrawRectangle(Point start, Size size, double thickness, bool fill = false)
        {
            var paint = new SKPaint
            {
                Color = Color,
                IsAntialias = true,
                Style = fill ? SKPaintStyle.StrokeAndFill : SKPaintStyle.Stroke,
                StrokeWidth = (float)thickness,
                StrokeCap = SKStrokeCap.Square,
            };

            surface.Canvas.DrawRect((float)start.X, (float)start.Y, (float)size.Width, (float)size.Height, paint);
        }

        public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            var paint = new SKPaint
            {
                Color = Color,
                IsAntialias = true,
                Style = fill ? SKPaintStyle.StrokeAndFill : SKPaintStyle.Stroke,
                StrokeWidth = (float)thickness,
                StrokeCap = SKStrokeCap.Square,
            };

            surface.Canvas.DrawOval(centre.ToSkPoint(), new SKSize((float)radiusX, (float)radiusY), paint);
        }

        public void DrawPath(Point start, IList<IPathCommand> commands, double thickness, bool fill = false)
        {
            var s = start.ToSkPoint();

            var path = new SKPath();
            path.MoveTo(s);
            foreach (var c in commands)
            {
                switch (c)
                {
                    case LineTo line:
                    {
                        path.LineTo(s + line.End.ToSkPoint());
                        break;
                    }
                    case CurveTo curve:
                    {
                        path.CubicTo(s + curve.ControlStart.ToSkPoint(), s + curve.ControlEnd.ToSkPoint(), s + curve.End.ToSkPoint());
                        break;
                    }
                    case MoveTo move:
                    {
                        path.MoveTo(s + move.End.ToSkPoint());
                        break;
                    }
                    case QuadraticBeizerCurveTo curve:
                    {
                        path.QuadTo(s + curve.Control.ToSkPoint(), s + curve.End.ToSkPoint());
                        break;
                    }
                    case EllipticalArcTo arc:
                    {
                        path.ArcTo((float)arc.Size.Width,
                                   (float)arc.Size.Height,
                                   (float)arc.RotationAngle,
                                   arc.IsLargeArc ? SKPathArcSize.Large : SKPathArcSize.Small,
                                   arc.SweepDirection == SweepDirection.Clockwise ? SKPathDirection.Clockwise : SKPathDirection.CounterClockwise,
                                   s.X + (float)arc.End.X,
                                   s.Y + (float)arc.End.Y);
                        break;
                    }
                    case ClosePath close:
                    {
                        path.Close();
                        break;
                    }
                    default:
                    {
                        path.MoveTo(s + c.End.ToSkPoint());
                        break;
                    }
                }
            }

            var paint = new SKPaint
            {
                Color = Color,
                IsAntialias = true,
                Style = fill ? SKPaintStyle.StrokeAndFill : SKPaintStyle.Stroke,
                StrokeWidth = (float)thickness,
                StrokeCap = SKStrokeCap.Square,
            };

            surface.Canvas.DrawPath(path, paint);
        }

        public void DrawText(Point anchor, TextAlignment alignment, IList<TextRun> textRuns)
        {
            var paint = new SKPaint
            {
                Color = Color,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextSize = 12f,
            };

            var subPaint = paint;

            float totalWidth = 0f;
            float totalHeight = 0f;

            foreach (TextRun run in textRuns)
            {
                var renderPaint = paint;
                if (run.Formatting.FormattingType == TextRunFormattingType.Subscript)
                    renderPaint = subPaint;
                var bounds = new SKRect();
                renderPaint.MeasureText(Encoding.UTF8.GetBytes(run.Text), ref bounds);
                totalWidth += bounds.Width;
                totalHeight = Math.Max(totalHeight, bounds.Height);
            }

            var startLocation = anchor.ToSkPoint();
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
                var renderPaint = paint;
                var renderLocation = new SKPoint(startLocation.X + horizontalOffsetCounter, startLocation.Y);

                if (run.Formatting.FormattingType == TextRunFormattingType.Subscript)
                {
                    renderPaint = subPaint;
                    renderLocation.X = renderLocation.X + 3f;
                }
                else if (run.Formatting.FormattingType == TextRunFormattingType.Superscript)
                {
                    renderPaint = subPaint;
                    renderLocation.X = renderLocation.X - 3f;
                }

                var bounds = new SKRect();
                renderPaint.MeasureText(run.Text, ref bounds);
                surface.Canvas.DrawText(run.Text, renderLocation.X, renderLocation.Y - bounds.Top, renderPaint);
                horizontalOffsetCounter += bounds.Width;
            }
        }

        public void Mutate(Action<SKCanvas> action)
        {
            action(surface.Canvas);
        }

        public void WriteAsPng(Stream stream)
        {
            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                data.SaveTo(stream);
            }
        }
    }
}
