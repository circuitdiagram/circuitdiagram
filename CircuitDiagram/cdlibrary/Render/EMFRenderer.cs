using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using CircuitDiagram.Components.Render;
using System.Drawing.Drawing2D;
using CircuitDiagram.Render.Path;

namespace CircuitDiagram.Render
{
    public class EMFRenderer : IRenderContext
    {
        [DllImport("gdi32")]
        extern static uint GetEnhMetaFileBits(IntPtr hemf, uint cbBuffer, byte[] lpbBuffer);

        Metafile m_metafile;
        Graphics m_graphics;
        MemoryStream m_stream;

        Pen m_pen;

        public bool Absolute { get { return true; } }

        public EMFRenderer(int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            m_graphics = Graphics.FromImage(bmp);
            m_stream = new MemoryStream();
            m_metafile = new Metafile(m_graphics.GetHdc(), new RectangleF(0, 0, width, height), MetafileFrameUnit.Pixel);
            m_graphics.ReleaseHdc();
            m_graphics.Dispose();
            bmp.Dispose();
            m_graphics = Graphics.FromImage(m_metafile);
        }

        public void Begin()
        {
            m_pen = new Pen(System.Drawing.Color.Black, 2f);
            m_pen.SetLineCap(System.Drawing.Drawing2D.LineCap.Square, System.Drawing.Drawing2D.LineCap.Square, System.Drawing.Drawing2D.DashCap.Flat);
        }

        public void End()
        {
        }

        public void StartSection(object tag)
        {
            // Do nothing.
        }

        public void WriteEnhMetafile(Stream stream)
        {
            m_graphics.Dispose();
            IntPtr h = m_metafile.GetHenhmetafile();
            uint size = GetEnhMetaFileBits(h, 0, null);
            byte[] data = new byte[size];
            GetEnhMetaFileBits(h, size, data);
            stream.Write(data, 0, (int)size);
        }

        public void DrawLine(System.Windows.Point start, System.Windows.Point end, double thickness)
        {
            m_pen.Width = (float)thickness;
            m_graphics.DrawLine(m_pen, (float)start.X, (float)start.Y, (float)end.X, (float)end.Y);
        }

        public void DrawRectangle(System.Windows.Point start, System.Windows.Size size, double thickness, bool fill = false)
        {
            m_pen.Width = (float)thickness;
            if (fill)
                m_graphics.FillRectangle(Brushes.Black, (int)start.X, (int)start.Y, (int)size.Width, (int)size.Height);
            m_graphics.DrawRectangle(m_pen, (int)start.X, (int)start.Y, (int)size.Width, (int)size.Height);
        }

        public void DrawEllipse(System.Windows.Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            m_pen.Width = (float)thickness;
            if (fill)
                m_graphics.FillRectangle(Brushes.Black, (int)(centre.X - radiusX), (int)(centre.Y - radiusY), 2 * (int)radiusX, 2 * (int)radiusY);
            m_graphics.DrawEllipse(m_pen, (int)(centre.X - radiusX), (int)(centre.Y - radiusY), 2 * (int)radiusX, 2 * (int)radiusY);
        }

        public void DrawPath(System.Windows.Point start, IList<IPathCommand> commands, double thickness, bool fill = false)
        {
            m_pen.Width = (float)thickness;

            GraphicsPath path = new GraphicsPath();
            System.Windows.Point previous = start;
            foreach (var command in commands)
            {
                switch (command.Type)
                {
                    case CommandType.MoveTo:
                        {
                            path.StartFigure();
                            break;
                        }
                    case CommandType.LineTo:
                        {
                            LineTo line = command as LineTo;
                            path.AddLine((float)previous.X, (float)previous.Y, (float)line.X + (float)start.X, (float)line.Y + (float)start.Y);
                            break;
                        }
                    case CommandType.CurveTo:
                        {
                            CurveTo curveTo = command as CurveTo;
                            
                            break;
                        }
                    case CommandType.EllipticalArcTo:
                        {
                            EllipticalArcTo ellipticalArcTo = command as EllipticalArcTo;
                            //path.AddArc((float)previous.X, (float)previous.Y, (float)ellipticalArcTo.Size.Width, (float)ellipticalArcTo.Size.Height, (float)ellipticalArcTo.RotationAngle, (float)ellipticalArcTo.RotationAngle);
                            break;
                        }
                    case CommandType.QuadraticBeizerCurveTo:
                        {
                            QuadraticBeizerCurveTo qbCurveTo = command as QuadraticBeizerCurveTo;
                            //path.AddBezier((float)previous.X, (float)previous.Y, (float)qbCurveTo.Control.X, (float)qbCurveTo.Control.Y, (float)qbCurveTo.Control.X, (float)qbCurveTo.Control.Y, (float)qbCurveTo.End.X, (float)qbCurveTo.End.Y);
                            break;
                        }
                }

                previous = new System.Windows.Point(start.X + command.End.X, start.Y + command.End.Y);
            }

            if (fill)
                m_graphics.FillPath(Brushes.Black, path);
            m_graphics.DrawPath(m_pen, path);
        }

        public void DrawText(System.Windows.Point anchor, TextAlignment alignment, IEnumerable<TextRun> textRuns)
        {
            float totalWidth = 0f;
            float totalHeight = 0f;
            
            foreach (TextRun run in textRuns)
            {
                Font font = new Font("Arial", (float)run.Formatting.Size / (run.Formatting.FormattingType == TextRunFormattingType.Subscript ? 1.5f : 1f));
                SizeF runSize = m_graphics.MeasureString(run.Text, font, 1000, StringFormat.GenericTypographic);
                totalWidth += runSize.Width;
                if (font.Height / 2 > totalHeight)
                    totalHeight = font.Height / 2;
            }

            PointF renderLocation = new PointF((float)anchor.X, (float)anchor.Y);
            if (alignment == TextAlignment.TopCentre || alignment == TextAlignment.CentreCentre || alignment == TextAlignment.BottomCentre)
                renderLocation.X -= totalWidth / 2;
            else if (alignment == TextAlignment.TopRight || alignment == TextAlignment.CentreRight || alignment == TextAlignment.BottomRight)
                renderLocation.X -= totalWidth;
            if (alignment == TextAlignment.CentreLeft || alignment == TextAlignment.CentreCentre || alignment == TextAlignment.CentreRight)
                renderLocation.Y -= totalHeight / 2;
            else if (alignment == TextAlignment.BottomLeft || alignment == TextAlignment.BottomCentre || alignment == TextAlignment.BottomRight)
                renderLocation.Y -= totalHeight;
            
            float horizontalOffsetCounter = 0;
            foreach (TextRun run in textRuns)
            {
                if (run.Formatting.FormattingType == TextRunFormattingType.Normal)
                {
                    m_graphics.DrawString(run.Text, new Font("Arial", (float)run.Formatting.Size), Brushes.Black, PointF.Add(renderLocation, new Size((int)horizontalOffsetCounter, 0)));
                    SizeF runSize = m_graphics.MeasureString(run.Text, new Font("Arial", (float)run.Formatting.Size), 1000, StringFormat.GenericTypographic);
                    horizontalOffsetCounter += runSize.Width;
                }
                else if (run.Formatting.FormattingType == TextRunFormattingType.Subscript)
                {
                    SizeF runSize = m_graphics.MeasureString(run.Text, new Font("Arial", (float)run.Formatting.Size / 1.5f), 1000, StringFormat.GenericTypographic);
                    m_graphics.DrawString(run.Text, new Font("Arial", (float)run.Formatting.Size / 1.5f), Brushes.Black, PointF.Add(renderLocation, new Size((int)horizontalOffsetCounter, (int)totalHeight)));
                    horizontalOffsetCounter += runSize.Width;
                }
                else if (run.Formatting.FormattingType == TextRunFormattingType.Superscript)
                {
                    
                }
            }
        }
    }
}
