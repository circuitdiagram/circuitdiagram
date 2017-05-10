using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Render.Path;
using ImageSharp;
using ImageSharp.Drawing.Brushes;
using ImageSharp.Drawing.Pens;
using ImageSharp.Formats;
using ImageSharp.PixelFormats;
using Point = CircuitDiagram.Primitives.Point;
using Size = CircuitDiagram.Primitives.Size;

namespace CircuitDiagram.Render
{
    public class BitmapDrawingContext : IDrawingContext
    {
        private readonly SolidBrush<Argb32> Black = new SolidBrush<Argb32>(NamedColors<Argb32>.Black);

        private readonly Image<Argb32> image;

        public BitmapDrawingContext(int width, int height)
        {
            image = new Image<Argb32>(width, height);
        }

        public void Begin()
        {
            // Do nothing
        }

        public void End()
        {
            // Do nothing
        }

        public void StartSection(object tag)
        {
            // Do nothing
        }

        public void DrawLine(Point start, Point end, double thickness)
        {
            image.DrawLines(Black, 2.0f, new[]
            {
                new Vector2((float)start.X, (float)start.Y),
                new Vector2((float)end.X, (float)end.Y)
            });
        }

        public void DrawRectangle(Point start, Size size, double thickness, bool fill = false)
        {
            image.DrawPolygon(Black, 2.0f, new[]
            {
                new Vector2((float)start.X, (float)start.Y),
                new Vector2((float)start.X + (float)size.Width, (float)start.Y),
                new Vector2((float)start.X + (float)size.Width, (float)start.Y + (float)size.Height),
                new Vector2((float)start.X, (float)start.Y + (float)size.Height) 
            });
        }

        public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            // Not supported
        }

        public void DrawPath(Point start, IList<IPathCommand> commands, double thickness, bool fill = false)
        {
            // Not supported
        }

        public void DrawText(Point anchor, TextAlignment alignment, IEnumerable<TextRun> textRuns)
        {
            // Not supported
        }

        public void WriteAsPng(Stream stream)
        {
            image.Save(stream, new PngEncoder());
        }
    }
}
