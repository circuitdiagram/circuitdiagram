using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram.EComponents
{
    public class Counter : EComponent
    {
        const double RectStart = 16d;

        public Counter()
        {
        }

        public override void Render(IRenderer dc, Color color)
        {
            dc.DrawLine(color, 2.0f, StartLocation, new Point(StartLocation.X + 9f, StartLocation.Y));
            dc.DrawRectangle(Colors.Transparent, color, 2f, new Rect(StartLocation.X + RectStart, StartLocation.Y - 25f, 120f, 50f));
            dc.DrawEllipse(Colors.Transparent, color, 2f, new Point(StartLocation.X + 11f, StartLocation.Y), 3d, 3d);
            dc.DrawPath(Colors.Transparent, color, 1f, String.Format("M {0} l 6,6 l -6,6", new Point(StartLocation.X + RectStart, StartLocation.Y - 6f).ToString()));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 15f, StartLocation.Y + 50f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 30f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 15f + 30f, StartLocation.Y + 50f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 60f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 15f + 60f, StartLocation.Y + 50f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 90f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 15f + 90f, StartLocation.Y + 50f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 120f, StartLocation.Y), EndLocation);
            dc.DrawText("A", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f, StartLocation.Y + 10f));
            dc.DrawText("B", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 30f, StartLocation.Y + 10f));
            dc.DrawText("C", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 60f, StartLocation.Y + 10f));
            dc.DrawText("D", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 90f, StartLocation.Y + 10f));
            dc.DrawText("R", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 108f, StartLocation.Y - 6f));
        }

        public override bool Intersects(Point point)
        {
            Rect intersectRect = new Rect(StartLocation.X + RectStart, StartLocation.Y - 25f, 120f, 50f);
            return intersectRect.IntersectsWith(new Rect(point, new Size(1f,1f)));
        }
    }
}
