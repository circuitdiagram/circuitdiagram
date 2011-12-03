using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CircuitDiagram.EComponents
{
    class Transformer : EComponent
    {
        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 20d), new Point(EndLocation.X, EndLocation.Y + 20d));
                else
                    return new Rect(new Point(StartLocation.X - 20d, StartLocation.Y), new Point(EndLocation.X + 20d, EndLocation.Y));
            }
        }

        public override void UpdateLayout()
        {
            this.ImplementMinimumSize(80d);
        }

        public override void Render(IRenderer dc, System.Windows.Media.Color colour)
        {
            if (Horizontal)
            {
                Point start = new Point(StartLocation.X + Size.Width / 2 - 24d, StartLocation.Y);
                Point end = new Point(StartLocation.X + Size.Width / 2 + 24d, StartLocation.Y);
                Point centre = new Point(StartLocation.X + Size.Width / 2, StartLocation.Y);
                dc.DrawLine(colour, 2d, new Point(StartLocation.X, StartLocation.Y - 20d), new Point(start.X, StartLocation.Y - 20d));
                dc.DrawLine(colour, 2d, new Point(StartLocation.X, StartLocation.Y + 20d), new Point(start.X, StartLocation.Y + 20d));
                dc.DrawLine(colour, 2d, new Point(end.X, end.Y - 20d), new Point(EndLocation.X, EndLocation.Y - 20d));
                dc.DrawLine(colour, 2d, new Point(end.X, end.Y + 20d), new Point(EndLocation.X, EndLocation.Y + 20d));
                dc.DrawLine(colour, 2d, new Point(centre.X - 5d, centre.Y - 20d), new Point(centre.X - 5d, centre.Y + 20d));
                dc.DrawLine(colour, 2d, new Point(centre.X + 5d, centre.Y - 20d), new Point(centre.X + 5d, centre.Y + 20d));
                dc.DrawPath(null, colour, 2d, String.Format("M {0} a 1.5,2 90 0 1 0,10 m 0,0 a 1.5,2 90 0 1 0,10 m 0,0 a 1.5,2 90 0 1 0,10 m 0,0 a 1.5,2 90 0 1 0,10 m 0,0", new Point(start.X, start.Y - 20d).ToString(System.Globalization.CultureInfo.InvariantCulture)));
                dc.DrawPath(null, colour, 2d, String.Format("M {0} a 1.5,2 90 0 0 0,10 m 0,0 a 1.5,2 90 0 0 0,10 m 0,0 a 1.5,2 90 0 0 0,10 m 0,0 a 1.5,2 90 0 0 0,10 m 0,0", new Point(end.X, end.Y - 20d).ToString(System.Globalization.CultureInfo.InvariantCulture)));
            }
            else
            {
                Point start = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 24d);
                Point end = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 24d);
                Point centre = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2);
                dc.DrawLine(colour, 2d, new Point(StartLocation.X - 20d, StartLocation.Y), new Point(StartLocation.X - 20d, start.Y));
                dc.DrawLine(colour, 2d, new Point(StartLocation.X + 20d, StartLocation.Y), new Point(StartLocation.X + 20d, start.Y));
                dc.DrawLine(colour, 2d, new Point(end.X - 20d, end.Y), new Point(EndLocation.X - 20d, EndLocation.Y));
                dc.DrawLine(colour, 2d, new Point(end.X + 20d, end.Y), new Point(EndLocation.X + 20d, EndLocation.Y));
                dc.DrawLine(colour, 2d, new Point(centre.X - 20d, centre.Y - 5d), new Point(centre.X + 20d, centre.Y - 5d));
                dc.DrawLine(colour, 2d, new Point(centre.X - 20d, centre.Y + 5d), new Point(centre.X + 20d, centre.Y + 5d));
                dc.DrawPath(null, colour, 2d, String.Format("M {0} a 2,1.5 90 0 0 10,0 m 0,0 a 2,1.5 90 0 0 10,0 m 0,0 a 2,1.5 90 0 0 10,0 m 0,0 a 2,1.5 90 0 0 10,0 m 0,0", new Point(start.X - 20d, start.Y).ToString(System.Globalization.CultureInfo.InvariantCulture)));
                dc.DrawPath(null, colour, 2d, String.Format("M {0} a 2,1.5 90 0 1 10,0 m 0,0 a 2,1.5 90 0 1 10,0 m 0,0 a 2,1.5 90 0 1 10,0 m 0,0 a 2,1.5 90 0 1 10,0 m 0,0", new Point(end.X - 20d, end.Y).ToString(System.Globalization.CultureInfo.InvariantCulture)));
            }
        }
    }
}
