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

namespace CircuitDiagram.Render
{
    public class WPFRenderer : IRenderContext
    {
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
            m_visual.Offset = new Vector(0,0);
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
            dc.DrawGeometry((fill ? Brushes.Black : Brushes.Transparent), new Pen(Brushes.Black, thickness), CircuitDiagram.Components.Render.Path.Path.GetGeometry(start, commands));
        }

        public void DrawText(Point anchor, Components.Render.TextAlignment alignment, string text, double size)
        {
            FormattedText formattedText = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), size, Brushes.Black);

            if (alignment == TextAlignment.TopCentre || alignment == TextAlignment.CentreCentre || alignment == TextAlignment.BottomCentre)
                anchor.X -= formattedText.Width / 2;
            else if (alignment == TextAlignment.TopRight || alignment == TextAlignment.CentreRight || alignment == TextAlignment.BottomRight)
                anchor.X -= formattedText.Width;
            if (alignment == TextAlignment.CentreLeft || alignment == TextAlignment.CentreCentre || alignment == TextAlignment.CentreRight)
                anchor.Y -= formattedText.Height / 2;
            else if (alignment == TextAlignment.BottomLeft || alignment == TextAlignment.BottomCentre || alignment == TextAlignment.BottomRight)
                anchor.Y -= formattedText.Height;

            dc.DrawText(formattedText, anchor);
        }
    }
}
