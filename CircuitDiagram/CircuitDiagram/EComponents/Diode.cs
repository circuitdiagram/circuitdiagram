using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CircuitDiagram.EComponents
{
    public class Diode : EComponent
    {
        public override double MinimumWidth
        {
            get
            {
                return 26.0f;
            }
        }

        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 8), new Size(EndLocation.X - StartLocation.X, 16));
                else
                    return new Rect(new Point(StartLocation.X - 8, StartLocation.Y), new Size(16, EndLocation.Y - StartLocation.Y));
            }
        }

        public Diode()
        {
        }

        public override void Render(IRenderer dc, Color color)
        {
            Point middle = new Point((StartLocation.X + EndLocation.X) / 2, (StartLocation.Y + EndLocation.Y) / 2);

            if (Horizontal)
            {
                dc.DrawLine(color, 2.0f, StartLocation, EndLocation);
                dc.DrawPath(null, color, 2.0f, String.Format("M {0},{1} m 0,-8 l 0,16 m 0,-8 l -15,8 l 0,-16 l 15,8", middle.X, middle.Y));
            }
            else
            {
                dc.DrawLine(color, 2.0f, StartLocation, EndLocation);
                dc.DrawPath(null, color, 2.0f, String.Format("M {0},{1} m -8,0 l 16,0 m-8,0 l 8,-15 l-16,0 l 8,15", middle.X, middle.Y));
            }
        }
    }
}
