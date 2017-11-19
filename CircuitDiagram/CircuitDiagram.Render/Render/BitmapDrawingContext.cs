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
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Brushes;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using SixLabors.Shapes;
using Point = CircuitDiagram.Primitives.Point;
using Size = CircuitDiagram.Primitives.Size;

namespace CircuitDiagram.Render
{
    public class BitmapDrawingContext : IDrawingContext
    {
        private static readonly SolidBrush<Argb32> Black = new SolidBrush<Argb32>(NamedColors<Argb32>.Black);
        
        private readonly Image<Argb32> image;
        private readonly bool ownsImage;

        public BitmapDrawingContext(int width, int height)
        {
            image = new Image<Argb32>(width, height);
            ownsImage = true;
        }

        public BitmapDrawingContext(Image<Argb32> target)
        {
            image = target;
            ownsImage = false;
        }
        
        public void DrawLine(Point start, Point end, double thickness)
        {
            image.Mutate(ctx => ctx.DrawLines(Black, 2.0f, new[]
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
            // Not supported
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
                        builder.AddBezier(currentPoint, curve.ControlStart.ToPointF(), curve.ControlEnd.ToPointF(), curve.End.ToPointF());
                        break;
                    }
                    case MoveTo move:
                    {
                        builder.StartFigure();
                        break;
                    }
                    case QuadraticBeizerCurveTo curve:
                    {
                        builder.AddBezier(currentPoint, curve.Control.ToPointF(), curve.End.ToPointF());
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

        public void DrawText(Point anchor, TextAlignment alignment, IList<TextRun> textRuns)
        {
            var font = new Font(SystemFonts.Find("Arial"), 12);
            image.Mutate(ctx =>
            {
                foreach (var textRun in textRuns)
                {
                    ctx.DrawText(textRun.Text, font, Black, anchor.ToPointF());
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
