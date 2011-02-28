using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram.EComponents
{
    public class SegDecoder : EComponent
    {
        const double RectStart = 16d;

        public SegDecoder()
        {
        }

        public override void Render(IRenderer dc, Color color)
        {
            dc.DrawRectangle(Colors.Transparent, color, 2f, new Rect(StartLocation.X + RectStart, StartLocation.Y - 25f, 120f, 50f));

            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f, StartLocation.Y - 25f), new Point(StartLocation.X + RectStart + 15f, StartLocation.Y - 50f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 30f, StartLocation.Y - 25f), new Point(StartLocation.X + RectStart + 15f + 30f, StartLocation.Y - 50f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 60f, StartLocation.Y - 25f), new Point(StartLocation.X + RectStart + 15f + 60f, StartLocation.Y - 50f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 90f, StartLocation.Y - 25f), new Point(StartLocation.X + RectStart + 15f + 90f, StartLocation.Y - 50f));
            dc.DrawText("A", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f, StartLocation.Y - 22f));
            dc.DrawText("B", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 30f, StartLocation.Y - 22f));
            dc.DrawText("C", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 60f, StartLocation.Y - 22f));
            dc.DrawText("D", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 90f, StartLocation.Y - 22f));

            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 15f, StartLocation.Y + 60f)); // a
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 15f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 15f + 15f, StartLocation.Y + 50f)); // b
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 30f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 15f + 30f, StartLocation.Y + 40f)); // c
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 45f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 15f + 45f, StartLocation.Y + 70f)); // d
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 60f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 15f + 60f, StartLocation.Y + 40f)); // e
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 75f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 15f + 75f, StartLocation.Y + 50f)); // f
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 90f, StartLocation.Y + 25f), new Point(StartLocation.X + RectStart + 15f + 90f, StartLocation.Y + 60f)); // g
            dc.DrawText("a", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f, StartLocation.Y + 10f));
            dc.DrawText("b", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 15f, StartLocation.Y + 10f));
            dc.DrawText("c", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 30f, StartLocation.Y + 10f));
            dc.DrawText("d", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 45f, StartLocation.Y + 10f));
            dc.DrawText("e", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 60f, StartLocation.Y + 10f));
            dc.DrawText("f", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 75f, StartLocation.Y + 10f));
            dc.DrawText("g", "Arial", 11d, color, new Point(StartLocation.X + RectStart + 10f + 90f, StartLocation.Y + 10f));
            // a
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f - 1f, StartLocation.Y + 60f), new Point(StartLocation.X + RectStart + 15f + 15f, StartLocation.Y + 60f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 15f, StartLocation.Y + 60f - 1f), new Point(StartLocation.X + RectStart + 15f + 15f, StartLocation.Y + 70f));
            // b
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 15f - 1f, StartLocation.Y + 50f), new Point(StartLocation.X + RectStart + 15f + 25f, StartLocation.Y + 50f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 25f, StartLocation.Y + 50f - 1f), new Point(StartLocation.X + RectStart + 15f + 25f, StartLocation.Y + 70f));
            // c
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 30f - 1f, StartLocation.Y + 40f), new Point(StartLocation.X + RectStart + 15f + 35f, StartLocation.Y + 40f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 35f, StartLocation.Y + 40f - 1f), new Point(StartLocation.X + RectStart + 15f + 35f, StartLocation.Y + 70f));
            // e
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 60f + 1f, StartLocation.Y + 40f), new Point(StartLocation.X + RectStart + 15f + 55f, StartLocation.Y + 40f));
            dc.DrawLine(color, 2f, new Point(StartLocation.X + RectStart + 15f + 55f, StartLocation.Y + 40f - 1f), new Point(StartLocation.X + RectStart + 15f + 55f, StartLocation.Y + 70f));
            //f + g
            dc.DrawPath(null, color, 2f, "M " + new Point(StartLocation.X + RectStart + 15f + 75f + 1f, StartLocation.Y + 50f).ToString() + " l -11,0 m 1,0 l 0,20 m 25,-10 l -16,0 m 1,0 l 0,10");
        }

        public override bool Intersects(Point point)
        {
            Rect intersectRect = new Rect(StartLocation.X + RectStart, StartLocation.Y - 25f, 120f, 50f);
            return intersectRect.IntersectsWith(new Rect(point, new Size(1f,1f)));
        }
    }
}