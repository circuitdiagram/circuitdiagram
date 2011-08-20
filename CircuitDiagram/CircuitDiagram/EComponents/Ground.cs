using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CircuitDiagram.EComponents
{
    class Ground : EComponent
    {
        public Ground()
        {
            base.CanFlip = true;
        }

        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 9d), new Point(EndLocation.X, EndLocation.Y + 9d));
                else
                    return new Rect(new Point(StartLocation.X - 9d, StartLocation.Y), new Point(EndLocation.X + 9d, EndLocation.Y));
            }
        }

        public override void Render(IRenderer dc, System.Windows.Media.Color colour)
        {
            if (Horizontal)
            {
                if (!IsFlipped)
                {
                    dc.DrawLine(colour, 2d, new Point(StartLocation.X + 14d, StartLocation.Y), EndLocation);
                    dc.DrawLine(colour, 2d, new Point(StartLocation.X + 14d, StartLocation.Y - 9d), new Point(StartLocation.X + 14d, StartLocation.Y + 9d));
                    dc.DrawLine(colour, 2d, new Point(StartLocation.X + 7d, StartLocation.Y - 5d), new Point(StartLocation.X + 7d, StartLocation.Y + 5d));
                    dc.DrawLine(colour, 2d, new Point(StartLocation.X, StartLocation.Y - 2d), new Point(StartLocation.X, StartLocation.Y + 2d));
                }
                else
                {
                    dc.DrawLine(colour, 2d, StartLocation, new Point(EndLocation.X - 14d, EndLocation.Y));
                    dc.DrawLine(colour, 2d, new Point(EndLocation.X - 14d, EndLocation.Y - 9d), new Point(EndLocation.X - 14d, EndLocation.Y + 9d));
                    dc.DrawLine(colour, 2d, new Point(EndLocation.X - 7d, EndLocation.Y - 5d), new Point(EndLocation.X - 7d, EndLocation.Y + 5d));
                    dc.DrawLine(colour, 2d, new Point(EndLocation.X, EndLocation.Y - 2d), new Point(EndLocation.X, EndLocation.Y + 2d));
                }
            }
            else
            {
                if (!IsFlipped)
                {
                    dc.DrawLine(colour, 2d, StartLocation, new Point(EndLocation.X, EndLocation.Y - 14d));
                    dc.DrawLine(colour, 2d, new Point(StartLocation.X - 9d, EndLocation.Y - 14d), new Point(StartLocation.X + 9d, EndLocation.Y - 14d));
                    dc.DrawLine(colour, 2d, new Point(StartLocation.X - 5d, EndLocation.Y - 7d), new Point(StartLocation.X + 5d, EndLocation.Y - 7d));
                    dc.DrawLine(colour, 2d, new Point(StartLocation.X - 2d, EndLocation.Y), new Point(StartLocation.X + 2d, EndLocation.Y));
                }
                else
                {
                    dc.DrawLine(colour, 2d, new Point(StartLocation.X, StartLocation.Y + 14d), EndLocation);
                    dc.DrawLine(colour, 2d, new Point(StartLocation.X - 9d, StartLocation.Y + 14d), new Point(StartLocation.X + 9d, StartLocation.Y + 14d));
                    dc.DrawLine(colour, 2d, new Point(StartLocation.X - 5d, StartLocation.Y + 7d), new Point(StartLocation.X + 5d, StartLocation.Y + 7d));
                    dc.DrawLine(colour, 2d, new Point(StartLocation.X - 2d, StartLocation.Y), new Point(StartLocation.X + 2d, StartLocation.Y));
                }
            }
        }
    }
}
