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

        private readonly FontFamily fontFamily;
        private readonly Image<Argb32> image;
        private readonly bool ownsImage;

        public BitmapDrawingContext(int width, int height)
        {
            fontFamily = SystemFonts.Find("Arial");
            image = new Image<Argb32>(width, height);
            ownsImage = true;
        }

        public BitmapDrawingContext(Image<Argb32> target)
        {
            fontFamily = SystemFonts.Find("Arial");
            image = target;
            ownsImage = false;
        }

        public BitmapDrawingContext(Image<Argb32> target, FontFamily fontFamily)
        {
            this.fontFamily = fontFamily;
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
                    var dimensions = TextMeasurer.MeasureBounds(run.Text, new RendererOptions(renderFont));
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

                    ctx.DrawText(run.Text, font, NamedColors<Argb32>.Black, renderLocation);
                    horizontalOffsetCounter += TextMeasurer.MeasureBounds(run.Text, new RendererOptions(renderFont)).Width;
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
