using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render.Path;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.Render
{
    public abstract class BufferedDrawingContext : IDrawingContext
    {
        private readonly IDrawingContext underlying;

        public BufferedDrawingContext()
            : this(null)
        {
        }

        public BufferedDrawingContext(IDrawingContext underlying)
        {
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

        protected abstract Size MeasureText(TextRun text);

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
