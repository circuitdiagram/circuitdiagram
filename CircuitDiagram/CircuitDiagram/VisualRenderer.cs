using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace CircuitDiagram
{
    class VisualRenderer : IRenderer
    {
        public DrawingContext DrawingContext { get; set; }

        public void DrawLine(Color color, double thickness, Point point0, Point point1)
        {
            DrawingContext.DrawLine(new Pen(new SolidColorBrush(color), thickness), point0, point1);
        }

        public void DrawEllipse(Color fillColor, Color strokeColor, double strokeThickness, Point centre, double radiusX, double radiusY)
        {
            DrawingContext.DrawEllipse(new SolidColorBrush(fillColor), new Pen(new SolidColorBrush(strokeColor), strokeThickness), centre, radiusX, radiusY);
        }

        public void DrawRectangle(Color fillColor, Color strokeColor, double strokeThickness, Rect rectangle)
        {
            DrawingContext.DrawRectangle(new SolidColorBrush(fillColor), new Pen(new SolidColorBrush(strokeColor), strokeThickness), rectangle);
        }

        public void DrawText(string text, string fontName, double emSize, Color foreColor, Point origin)
        {
            DrawingContext.DrawText(new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(fontName), emSize, new SolidColorBrush(foreColor)), origin);
        }

        public void DrawPath(Color? fillColor, Color strokeColor, double thickness, string path)
        {
            Geometry geometry = Geometry.Parse(path);
            DrawingContext.DrawGeometry((fillColor.HasValue ? new SolidColorBrush(fillColor.Value) : null), new Pen(new SolidColorBrush(strokeColor), thickness), geometry);
        }
    }
}
