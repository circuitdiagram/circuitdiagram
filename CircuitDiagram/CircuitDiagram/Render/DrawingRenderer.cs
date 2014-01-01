using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using TextAlignment = CircuitDiagram.Render.TextAlignment;
using CircuitDiagram.Components.Description.Render;
using CircuitDiagram.Render.Path;

namespace CircuitDiagram.Render
{
    class DrawingRenderer : IRenderContext
    {
        public bool Absolute { get { return false; } }

        public DrawingContext Context { get; private set; }
        private Pen Pen { get; set; }

        public DrawingRenderer(DrawingContext context)
        {
            Context = context;
        }

        public void Begin()
        {
            Pen = new Pen(Brushes.Black, 2d);
            Pen.StartLineCap = PenLineCap.Square;
            Pen.EndLineCap = PenLineCap.Square;
        }

        public void End()
        {
        }

        public void StartSection(object tag)
        {
            // Do nothing
        }

        public void DrawLine(Point start, Point end, double thickness)
        {
            Pen newPen = new System.Windows.Media.Pen(Brushes.Black, thickness);
            newPen.StartLineCap = PenLineCap.Square;
            newPen.EndLineCap = PenLineCap.Square;
            Context.DrawLine(newPen, start, end);
        }

        public void DrawRectangle(Point start, Size size, double thickness, bool fill = false)
        {
            Pen.Thickness = thickness;
            Context.DrawRectangle(fill ? Brushes.Black : null, Pen, new Rect(start, size));
        }

        public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            Pen.Thickness = thickness;
            Context.DrawEllipse(fill ? Brushes.Black : null, Pen, centre, radiusX, radiusY);
        }

        public void DrawPath(Point start, IList<IPathCommand> commands, double thickness, bool fill = false)
        {
            Pen newPen = new System.Windows.Media.Pen(Brushes.Black, thickness);
            newPen.StartLineCap = PenLineCap.Square;
            newPen.EndLineCap = PenLineCap.Square;
            Context.DrawGeometry((fill ? Brushes.Black : null), newPen, RenderHelper.GetGeometry(start, commands, fill));
        }

        public void DrawText(Point anchor, TextAlignment alignment, IEnumerable<TextRun> textRuns)
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
                    Context.DrawText(formattedText, Point.Add(renderLocation, new Vector(horizontalOffsetCounter, 0d)));
                    horizontalOffsetCounter += formattedText.Width;
                }
                else if (run.Formatting.FormattingType == TextRunFormattingType.Subscript)
                {
                    FormattedText formattedText = new FormattedText(run.Text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(RenderHelper.CircuitFont()), run.Formatting.Size / 1.5, Brushes.Black);
                    Context.DrawText(formattedText, Point.Add(renderLocation, new Vector(horizontalOffsetCounter, totalHeight - formattedText.Height)));
                    horizontalOffsetCounter += formattedText.Width;
                }
                else if (run.Formatting.FormattingType == TextRunFormattingType.Superscript)
                {
                    FormattedText formattedText = new FormattedText(run.Text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(RenderHelper.CircuitFont()), run.Formatting.Size / 1.5, Brushes.Black);
                    Context.DrawText(formattedText, Point.Add(renderLocation, new Vector(horizontalOffsetCounter, -3d)));
                    horizontalOffsetCounter += formattedText.Width;
                }
            }
        }
    }
}
