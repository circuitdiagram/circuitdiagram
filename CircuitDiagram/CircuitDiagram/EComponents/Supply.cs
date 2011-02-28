using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CircuitDiagram.EComponents
{
    public class Supply : EComponent
    {
        const double Gap = 6.0d;
        const double PlusTerminalLength = 20d;
        const double MinusTerminalLength = 10d;

        public bool TlToBr = true;

        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public override bool Intersects(Point point)
        {
            Rect thisRect = new Rect(StartLocation, EndLocation - StartLocation);
            return thisRect.IntersectsWith(new Rect(point, new Size(1, 1)));
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (Horizontal)
            {
                dc.DrawLine(color, 2.0f, StartLocation, new Point(StartLocation.X + Size.Width / 2 - 0.5 * Gap, StartLocation.Y));
                dc.DrawLine(color, 2.0f, new Point(StartLocation.X + Size.Width / 2 - 0.5 * Gap, StartLocation.Y - 0.5 * PlusTerminalLength), new Point(StartLocation.X + Size.Width / 2 - 0.5 * Gap, StartLocation.Y + 0.5 * PlusTerminalLength));
                dc.DrawLine(color, 3.0f, new Point(StartLocation.X + Size.Width / 2 + 0.5 * Gap, StartLocation.Y - 0.5 * MinusTerminalLength), new Point(StartLocation.X + Size.Width / 2 + 0.5 * Gap, StartLocation.Y + 0.5 * MinusTerminalLength));
                dc.DrawLine(color, 2.0f, new Point(StartLocation.X + Size.Width / 2 + 0.5 * Gap, EndLocation.Y), EndLocation);
            }
            else
            {
                dc.DrawLine(color, 2.0f, StartLocation, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 0.5 * Gap));
                dc.DrawLine(color, 2.0f, new Point(StartLocation.X - 0.5 * PlusTerminalLength, StartLocation.Y + Size.Height / 2 - 0.5 * Gap), new Point(StartLocation.X + 0.5 * PlusTerminalLength, StartLocation.Y + Size.Height / 2 - 0.5 * Gap));
                dc.DrawLine(color, 3.0f, new Point(StartLocation.X - 0.5 * MinusTerminalLength, StartLocation.Y + Size.Height / 2 + 0.5 * Gap), new Point(StartLocation.X + 0.5 * MinusTerminalLength, StartLocation.Y + Size.Height / 2 + 0.5 * Gap));
                dc.DrawLine(color, 2.0f, new Point(EndLocation.X, EndLocation.Y - Size.Height / 2 + 0.5 * Gap), EndLocation);
            }
        }
    }
}