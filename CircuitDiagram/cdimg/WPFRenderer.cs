// WPFRenderer.cs
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
using TextAlignment = CircuitDiagram.Components.Render.TextAlignment;
using System.Windows.Media.Imaging;
using CircuitDiagram.Components.Render;
using CircuitDiagram.Components.Render.Path;

namespace CircuitDiagram.Render
{
    public class WPFRenderer : IRenderContext
    {
        public bool Absolute { get { return true; } }

        DrawingVisual m_visual;
        DrawingContext dc;

        public DrawingVisual DrawingVisual { get { return m_visual; } }

        public WPFRenderer()
        {
            m_visual = new DrawingVisual();
        }

        public void Begin()
        {
            dc = m_visual.RenderOpen();
        }

        public void End()
        {
            dc.Close();
        }

        public System.IO.MemoryStream GetPNGImage(int width, int height, bool center = false)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);

            if (center)
                m_visual.Offset = new Vector(width / 2 - m_visual.ContentBounds.Width / 2 - m_visual.ContentBounds.Left, height / 2 - m_visual.ContentBounds.Height / 2 - m_visual.ContentBounds.Top);

            bitmap.Render(m_visual);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            System.IO.MemoryStream returnStream = new System.IO.MemoryStream();
            encoder.Save(returnStream);
            returnStream.Flush();
            m_visual.Offset = new Vector(0, 0);
            return returnStream;
        }

        public void DrawLine(Point start, Point end, double thickness)
        {
            dc.DrawLine(new Pen(Brushes.Black, thickness), start, end);
        }

        public void DrawRectangle(Point start, Size size, double thickness, bool fill = false)
        {
            dc.DrawRectangle((fill ? Brushes.Black : Brushes.Transparent), new Pen(Brushes.Black, thickness), new Rect(start, size));
        }

        public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            dc.DrawEllipse((fill ? Brushes.Black : Brushes.Transparent), new Pen(Brushes.Black, thickness), centre, radiusX, radiusY);
        }

        public void DrawPath(Point start, IList<Components.Render.Path.IPathCommand> commands, double thickness, bool fill = false)
        {
            dc.DrawGeometry((fill ? Brushes.Black : Brushes.Transparent), new Pen(Brushes.Black, thickness), RenderHelper.GetGeometry(start, commands, fill));
        }

        public void DrawText(Point anchor, Components.Render.TextAlignment alignment, IEnumerable<CircuitDiagram.Components.Render.TextRun> textRuns)
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

            Point renderLocation = anchor;
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
                    dc.DrawText(formattedText, Point.Add(renderLocation, new Vector(horizontalOffsetCounter, 0d)));
                    horizontalOffsetCounter += formattedText.Width;
                }
                else if (run.Formatting.FormattingType == TextRunFormattingType.Subscript)
                {
                    FormattedText formattedText = new FormattedText(run.Text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(RenderHelper.CircuitFont()), run.Formatting.Size / 1.5, Brushes.Black);
                    dc.DrawText(formattedText, Point.Add(renderLocation, new Vector(horizontalOffsetCounter, totalHeight - formattedText.Height)));
                    horizontalOffsetCounter += formattedText.Width;
                }
                else if (run.Formatting.FormattingType == TextRunFormattingType.Superscript)
                {
                    FormattedText formattedText = new FormattedText(run.Text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(RenderHelper.CircuitFont()), run.Formatting.Size / 1.5, Brushes.Black);
                    dc.DrawText(formattedText, Point.Add(renderLocation, new Vector(horizontalOffsetCounter, -3d)));
                    horizontalOffsetCounter += formattedText.Width;
                }
            }
        }
    }

    static class RenderHelper
    {
        public static Geometry GetGeometry(Point start, IList<IPathCommand> commands, bool fill)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (var dc = geometry.Open())
            {
                dc.BeginFigure(start, fill, false);
                Vector startOffset = new Vector(start.X, start.Y);
                foreach (IPathCommand command in commands)
                {
                    command.Draw(dc, startOffset);
                }
                dc.Close();
            }
            return geometry;
        }

        public static void Draw(this IPathCommand element, StreamGeometryContext dc, Vector startOffset)
        {
            if (element is LineTo)
                dc.LineTo(new System.Windows.Point((element as LineTo).X + startOffset.X, (element as LineTo).Y + startOffset.Y), true, true);
            else if (element is MoveTo)
                dc.LineTo(new System.Windows.Point((element as MoveTo).X + startOffset.X, (element as MoveTo).Y + startOffset.Y), false, true);
            else if (element is CurveTo)
            {
                Point controlStart = Point.Add((element as CurveTo).ControlStart, startOffset);
                Point controlEnd = Point.Add((element as CurveTo).ControlEnd, startOffset);
                Point end = Point.Add((element as CurveTo).End, startOffset);
                dc.BezierTo(controlStart, controlEnd, end, true, true);
            }
            else if (element is EllipticalArcTo)
            {
                dc.ArcTo(Point.Add((element as EllipticalArcTo).End, startOffset), (element as EllipticalArcTo).Size, (element as EllipticalArcTo).RotationAngle, (element as EllipticalArcTo).IsLargeArc, (element as EllipticalArcTo).SweepDirection == Components.Render.Path.SweepDirection.Clockwise ? System.Windows.Media.SweepDirection.Clockwise : System.Windows.Media.SweepDirection.Counterclockwise, true, true);
            }
            else if (element is SmoothCurveTo)
            {
                SmoothCurveTo item = element as SmoothCurveTo;
                dc.BezierTo(Point.Add(item.ControlStart, startOffset), Point.Add(item.ControlEnd, startOffset), Point.Add(item.End, startOffset), true, true);
            }
            else if (element is SmoothQuadraticBeizerCurveTo)
            {
                SmoothQuadraticBeizerCurveTo item = element as SmoothQuadraticBeizerCurveTo;
                dc.BezierTo(Point.Add(item.ControlStart, startOffset), Point.Add(item.ControlEnd, startOffset), Point.Add(item.End, startOffset), true, true);
            }
            else if (element is QuadraticBeizerCurveTo)
            {
                QuadraticBeizerCurveTo item = element as QuadraticBeizerCurveTo;
                dc.QuadraticBezierTo(Point.Add(item.Control, startOffset), Point.Add(item.End, startOffset), true, true);
            }
        }

        public static string CircuitFont()
        {
            return "Arial";
        }
    }
}
