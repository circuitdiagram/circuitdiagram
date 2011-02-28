using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram.EComponents
{
    public class Capacitor : EComponent
    {
        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public double Capacitance { get; set; }

        private string CapacitanceString
        {

            get
            {
                if (Capacitance > 1)
                    return Capacitance.ToString() + " F";
                else if (Capacitance >= 0.001)
                    return Math.Round(Capacitance * 1000, 1).ToString() + " mF";
                else if (Capacitance >= 0.000001)
                    return (Capacitance * 1000000).ToString() + " \u00B5" + "F";
                else
                    return (Capacitance * 1000000000).ToString() + " pF";
            }
        }

        public Capacitor()
        {
            Capacitance = 1 * Math.Pow(10, -3);
        }

        public override void Initialize()
        {
            base.Editor = new CapacitorEditor();
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (Horizontal)
            {
                Point gapStart = new Point(StartLocation.X + Size.Width / 2 - 4, StartLocation.Y);
                Point gapEnd = new Point(StartLocation.X + Size.Width / 2 + 4, StartLocation.Y);
                dc.DrawLine(color, 2d, StartLocation, gapStart);
                dc.DrawLine(color, 2d, gapEnd, EndLocation);
                dc.DrawLine(color, 2d, Point.Add(gapStart, new Vector(0, -14)), Point.Add(gapStart, new Vector(0, 14)));
                dc.DrawLine(color, 2d, Point.Add(gapEnd, new Vector(0, -14)), Point.Add(gapEnd, new Vector(0, 14)));
                FormattedText text = new FormattedText(CapacitanceString, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10d, new SolidColorBrush(Colors.Black));
                dc.DrawText(CapacitanceString, "Arial", 10d, color, new Point(StartLocation.X + Size.Width / 2 - text.Width / 2, StartLocation.Y - 22d - text.Height / 2));
            }
            if (!Horizontal)
            {
                Point gapStart = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 4);
                Point gapEnd = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 4);
                dc.DrawLine(color, 2d, StartLocation, gapStart);
                dc.DrawLine(color, 2d, gapEnd, EndLocation);
                dc.DrawLine(color, 2d, Point.Add(gapStart, new Vector(-14, 0)), Point.Add(gapStart, new Vector(14, 0)));
                dc.DrawLine(color, 2d, Point.Add(gapEnd, new Vector(-14, 0)), Point.Add(gapEnd, new Vector(14, 0)));
                FormattedText text = new FormattedText(CapacitanceString, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10d, new SolidColorBrush(Colors.Black));
                dc.DrawText(CapacitanceString, "Arial", 10d, color, new Point(StartLocation.X - 22d - text.Width, StartLocation.Y + Size.Height / 2 - text.Height / 2));
            }
        }

        public override bool Intersects(Point point)
        {
            Rect capacitorRect = Horizontal ? new Rect(StartLocation.X + Size.Width / 2 - 4, StartLocation.Y - 14, 8, 28) :
                new Rect(StartLocation.X - 14, StartLocation.Y + Size.Height / 2 - 4, 28, 8);
            return capacitorRect.IntersectsWith(new Rect(point, new Size(1,1))) || base.Intersects(point);
        }
    }
}
