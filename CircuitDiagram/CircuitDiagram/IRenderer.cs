using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram
{
    public interface IRenderer
    {
        void DrawLine(Color color, double thickness, Point point0, Point point1);
        void DrawEllipse(Color fillColor, Color strokeColor, double strokeThickness, Point centre, double radiusX, double radiusY);
        void DrawRectangle(Color fillColor, Color strokeColor, double strokeThickness, Rect rectangle);
        void DrawText(string text, string fontName, double emSize, Color foreColor, Point origin);
        void DrawPath(Color? fillColor, Color strokeColor, double thickness, string path);
    }
}