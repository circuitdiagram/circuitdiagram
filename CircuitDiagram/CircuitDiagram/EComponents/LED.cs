using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram.EComponents
{
    public class LED : EComponent
    {
        public LED()
        {
        }

        public override void Render(IRenderer dc, Color color)
        {
            Point middle = new Point((StartLocation.X + EndLocation.X) / 2, (StartLocation.Y + EndLocation.Y) / 2);

            if (Horizontal)
            {
                dc.DrawLine(color, 2.0f, StartLocation, EndLocation);
            }
            else
            {
                dc.DrawLine(color, 2.0f, StartLocation, EndLocation);
                dc.DrawPath(null, color, 2.0f, String.Format("M {0},{1} m -8,0 l16,0 m-8,0 l 8,-15 l-16,0 l 8,15", middle.X, middle.Y));
                dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m 11,-2 l 8,8 m 1,1 l -2,-4 l -2,2 l 4,2 l -2,-4", middle.X, middle.Y));
                dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m 4,4 l 8,8 m 1,1 l -2,-4 l -2,2 l 4,2 l -2,-4", middle.X, middle.Y));
            }
        }
    }
}
