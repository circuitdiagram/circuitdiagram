using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CircuitDiagram
{
    static class ComponentHelper
    {
        public static void ImplementMinimumSize(this EComponent component, double size)
        {
            if (component.Horizontal && component.EndLocation.X - component.StartLocation.X < size)
                component.EndLocation = new Point(component.StartLocation.X + size, component.EndLocation.Y);
            else if (!component.Horizontal && component.EndLocation.Y - component.StartLocation.Y < size)
                component.EndLocation = new Point(component.EndLocation.X, component.StartLocation.Y + size);
        }

        public static Point Snap(Point point, double gridSize)
        {
            return new Point(Math.Round(point.X / gridSize) * gridSize, Math.Round(point.Y / gridSize) * gridSize);
        }

        public static string ConvertToSentenceCase(string text)
        {
            return text;
        }
    }
}
