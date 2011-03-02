using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram.EComponents
{
    public class Switch : EComponent
    {
        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public override double MinimumWidth
        {
            get
            {
                return 30.0d;
            }
        }

        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 15), new Size(EndLocation.X - StartLocation.X, 18));
                else
                    return new Rect(new Point(StartLocation.X - 15, StartLocation.Y), new Size(18, EndLocation.Y - StartLocation.Y));
            }
        }

        public Switch()
        {
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (Horizontal)
            {
                Point gapStart = new Point(StartLocation.X + Size.Width / 2 - 12, StartLocation.Y);
                Point gapEnd = new Point(StartLocation.X + Size.Width / 2 + 12, StartLocation.Y);
                dc.DrawLine(color, 2d, StartLocation, gapStart);
                dc.DrawEllipse(Colors.White, color, 1d, gapStart, 3d, 3d);
                dc.DrawLine(color, 2d, gapEnd, EndLocation);
                dc.DrawEllipse(Colors.White, color, 1d, gapEnd, 3d, 3d);
                dc.DrawPath(null, color, 2f, "M " + gapStart.ToString() + " m -2,-8 l 28,0 m -14,0 l 0,-6 m -6,0 l 12,0");
            }
            if (!Horizontal)
            {
                Point gapStart = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 12);
                dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " L " + gapStart.ToString() + " m 0,24 L" + EndLocation.ToString());
                dc.DrawEllipse(Colors.White, color, 1d, gapStart, 3d, 3d);
                dc.DrawEllipse(Colors.White, color, 1d, Point.Add(gapStart, new Vector(0, 24d)), 3d, 3d);
                dc.DrawPath(null, color, 2f, "M " + gapStart.ToString() + " m -8,-2 l 0,28 m 0,-14 l -6,0 m 0,-6 l 0,12");
            }
        }
    }
}
