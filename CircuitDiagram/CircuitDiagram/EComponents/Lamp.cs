using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CircuitDiagram.EComponents
{
    public class Lamp : EComponent
    {
        public override double MinimumWidth
        {
            get
            {
                return 30.0f;
            }
        }

        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 14), new Size(EndLocation.X - StartLocation.X, 28));
                else
                    return new Rect(new Point(StartLocation.X - 14, StartLocation.Y), new Size(28, EndLocation.Y - StartLocation.Y));
            }
        }

        public Lamp()
        {
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (Horizontal)
            {
                dc.DrawLine(color, 2.0f, StartLocation, new Point(StartLocation.X + Size.Width / 2 - 12d, StartLocation.Y));
                dc.DrawEllipse(Colors.Transparent, color, 2d, new Point(StartLocation.X + Size.Width / 2, StartLocation.Y), 12d, 12d);
                dc.DrawLine(color, 2d, new Point(StartLocation.X + Size.Width / 2 - 8d, StartLocation.Y - 8d), new Point(StartLocation.X + Size.Width / 2 + 8d, StartLocation.Y + 8d));
                dc.DrawLine(color, 2d, new Point(StartLocation.X + Size.Width / 2 + 8d, StartLocation.Y - 8d), new Point(StartLocation.X + Size.Width / 2 - 8d, StartLocation.Y + 8d));
                dc.DrawLine(color, 2.0f, new Point(StartLocation.X + Size.Width / 2 + 12d, StartLocation.Y), EndLocation);
            }
            else
            {
                dc.DrawLine(color, 2.0f, StartLocation, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 12d));
                dc.DrawEllipse(Colors.Transparent, color, 2d, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2), 12d, 12d);
                dc.DrawLine(color, 2d, new Point(StartLocation.X - 8d, StartLocation.Y + Size.Height / 2 - 8d), new Point(StartLocation.X + 8d, StartLocation.Y + Size.Height / 2 + 8d));
                dc.DrawLine(color, 2d, new Point(StartLocation.X + 8d, StartLocation.Y + Size.Height / 2 - 8d), new Point(StartLocation.X - 8d, StartLocation.Y + Size.Height / 2 + 8d));
                dc.DrawLine(color, 2.0f, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 12d), EndLocation);
            }
        }
    }
}
