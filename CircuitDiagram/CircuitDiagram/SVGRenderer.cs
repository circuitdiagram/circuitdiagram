using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram
{
    public class SVGRenderer : IRenderer
    {
        public SVGLibrary.SVGDocument SVGDocument { get; set; }

        public void DrawLine(Color color, double thickness, Point point0, Point point1)
        {
            SVGDocument.DrawLine(color, thickness, point0, point1);
        }

        public void DrawEllipse(Color fillColor, Color strokeColor, double strokeThickness, Point centre, double radiusX, double radiusY)
        {
            SVGDocument.DrawEllipse(fillColor, strokeColor, strokeThickness, centre, radiusX, radiusY);
        }

        public void DrawRectangle(Color fillColor, Color strokeColor, double strokeThickness, Rect rectangle)
        {
            SVGDocument.DrawRectangle(fillColor, strokeColor, strokeThickness, rectangle);
        }

        public void DrawText(string text, string fontName, double emSize, Color foreColor, Point origin)
        {
            FormattedText ft = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(fontName), emSize, new SolidColorBrush(foreColor));
            SVGDocument.DrawText(text, fontName, emSize, foreColor, Point.Add(origin, new Vector(0d, (ft.Height / 2) + 2d)));
        }

        public void DrawPath(Color? fillColor, Color strokeColor, double thickness, string path)
        {
            SVGDocument.DrawPath((fillColor.HasValue ? fillColor.Value : Colors.Transparent), strokeColor, thickness, path);
        }
    }
}
