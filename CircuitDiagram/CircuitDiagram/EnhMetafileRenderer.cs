using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;

namespace CircuitDiagram
{
    class EnhMetafileRenderer : IRenderer
    {
        [DllImport("gdi32")]
        extern static uint GetEnhMetaFileBits(IntPtr hemf, uint cbBuffer, byte[] lpbBuffer);

        Metafile m_metafile;
        Graphics m_graphics;
        MemoryStream m_stream;

        public EnhMetafileRenderer(int width, int height)
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

        public void SaveEnhMetafile(string path)
        {
            m_graphics.Dispose();
            IntPtr h = m_metafile.GetHenhmetafile();
            uint size = GetEnhMetaFileBits(h, 0, null);
            byte[] data = new byte[size];
            GetEnhMetaFileBits(h, size, data);
            using (FileStream w = File.Create(path))
            {
                w.Write(data, 0, (int)size);
            }
        }

        public void DrawLine(System.Windows.Media.Color color, double thickness, System.Windows.Point point0, System.Windows.Point point1)
        {
            m_graphics.DrawLine(new Pen(ConvertColor(color), (float)thickness), ConvertPoint(point0), ConvertPoint(point1));
        }

        public void DrawEllipse(System.Windows.Media.Color fillColor, System.Windows.Media.Color strokeColor, double strokeThickness, System.Windows.Point centre, double radiusX, double radiusY)
        {
            if (fillColor == System.Windows.Media.Colors.Transparent)
                m_graphics.DrawEllipse(new Pen(ConvertColor(strokeColor), (float)strokeThickness), (int)(centre.X - radiusX), (int)(centre.Y - radiusY), (int)radiusX * 2, (int)radiusY * 2);
            else
                m_graphics.FillEllipse(new SolidBrush(ConvertColor(fillColor)), (int)(centre.X - radiusX), (int)(centre.Y - radiusY), (int)radiusX * 2, (int)radiusY * 2);
        }

        public void DrawRectangle(System.Windows.Media.Color fillColor, System.Windows.Media.Color strokeColor, double strokeThickness, System.Windows.Rect rectangle)
        {
            if (fillColor != System.Windows.Media.Colors.Transparent)
                m_graphics.FillRectangle(new SolidBrush(ConvertColor(fillColor)), (float)rectangle.X, (float)rectangle.Y, (float)rectangle.Width, (float)rectangle.Height);
            m_graphics.DrawRectangle(new Pen(ConvertColor(strokeColor), (float)strokeThickness), (float)rectangle.X, (float)rectangle.Y, (float)rectangle.Width, (float)rectangle.Height);
        }

        public void DrawText(string text, string fontName, double emSize, System.Windows.Media.Color foreColor, System.Windows.Point origin)
        {
            m_graphics.DrawString(text, new Font(new FontFamily(fontName), (float)emSize), new SolidBrush(ConvertColor(foreColor)), (float)origin.X, (float)origin.Y);
        }

        public void DrawFormattedText(System.Windows.Media.FormattedText text, System.Windows.Point origin)
        {
        }

        public void DrawPath(System.Windows.Media.Color? fillColor, System.Windows.Media.Color strokeColor, double thickness, string path, double translateOffsetX = 0.0, double translateOffsetY = 0.0)
        {
            Regex regex = new Regex("[a-zA-Z]+[ \\-0-9,]+");
            MatchCollection collection = regex.Matches(path);
            System.Drawing.Point lastPoint = new Point();
            GraphicsPath gPath = new GraphicsPath();
            //gPath.StartFigure();
            foreach (Match match in collection)
            {
                char command = match.Value.Trim()[0];
                string[] parameters = match.Value.Trim().Substring(1).Split(new string[] {" ",","}, StringSplitOptions.RemoveEmptyEntries);
                switch (command)
                {
                    case 'M':
                        lastPoint = new Point(int.Parse(parameters[0]), int.Parse(parameters[1]));
                        break;
                    case 'm':
                        lastPoint = new Point(lastPoint.X + int.Parse(parameters[0]), lastPoint.Y + int.Parse(parameters[1]));
                        break;
                    case 'l':
                        {
                            Point newLastPoint = new Point(lastPoint.X + int.Parse(parameters[0]), lastPoint.Y + int.Parse(parameters[1]));
                            gPath.AddLine(lastPoint, newLastPoint);
                            lastPoint = newLastPoint;
                        }
                        break;
                    case 'L':
                        {
                            Point newLastPoint = new Point(int.Parse(parameters[0]), int.Parse(parameters[1]));
                            gPath.AddLine(lastPoint, newLastPoint);
                            lastPoint = newLastPoint;
                        }
                        break;
                }
            }
            //gPath.CloseFigure();
            m_graphics.DrawPath(new Pen(ConvertColor(strokeColor), (float)thickness), gPath);//new System.Drawing.Drawing2D.GraphicsPath(points.ToArray(), pointTypes.ToArray()));
        }

        public void DrawImage(System.Windows.Point location, System.Windows.Media.ImageSource image)
        {
        }

        static System.Drawing.Color ConvertColor(System.Windows.Media.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        static System.Drawing.Point ConvertPoint(System.Windows.Point point)
        {
            return new Point((int)point.X, (int)point.Y);
        }
    }
}
