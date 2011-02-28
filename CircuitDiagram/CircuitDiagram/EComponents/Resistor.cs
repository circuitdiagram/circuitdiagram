using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram.EComponents
{
    public class Resistor : EComponent
    {
        public double Resistance { get; set; }
        private string ResistanceString
        {
            get
            {
                if (Resistance < 1000)
                    return Resistance.ToString() + " \u2126";
                else if (Resistance < 1000000)
                    return Math.Round(Resistance / 1000, 1).ToString() + " k\u2126";
                else
                    return Math.Round(Resistance / 1000000, 1).ToString() +  "M\u2126";
            }
        }

        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public Resistor()
        {
            Resistance = 4700;
        }

        public override void Initialize()
        {
            base.Editor = new ResistorEditor();
        }

        public override bool Intersects(Point point)
        {
            if (Horizontal)
            {
                Rect thisRect = new Rect(StartLocation, new Point(StartLocation.X + Size.Width / 2 - 20d, StartLocation.Y));
                if (thisRect.IntersectsWith(new Rect(point, new Size(1, 1))))
                    return true;
                thisRect = new Rect(new Point(StartLocation.X + Size.Width / 2 + 20d, StartLocation.Y), EndLocation);
                if (thisRect.IntersectsWith(new Rect(point, new Size(1, 1))))
                    return true;
                thisRect = new Rect(StartLocation.X + Size.Width / 2 - 20d, StartLocation.Y - 8d, 40d, 16d);
                if (thisRect.IntersectsWith(new Rect(point, new Size(1, 1))))
                    return true;
                return false;
            }
            else
            {
                Rect thisRect = new Rect(StartLocation, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 20d));
                if (thisRect.IntersectsWith(new Rect(point, new Size(1, 1))))
                    return true;
                thisRect = new Rect(StartLocation.X - 8d, StartLocation.Y + Size.Height / 2 - 20d, 16d, 40d);
                if (thisRect.IntersectsWith(new Rect(point, new Size(1, 1))))
                    return true;
                thisRect = new Rect(new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 20d), EndLocation);
                if (thisRect.IntersectsWith(new Rect(point, new Size(1, 1))))
                    return true;
                return false;
            }
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (Horizontal)
            {
                dc.DrawLine(color, 2.0f, StartLocation, new Point(StartLocation.X + Size.Width / 2 - 20d, StartLocation.Y));
                dc.DrawRectangle(Color.FromArgb(0, 255, 255, 255), color, 2d, new Rect(StartLocation.X + Size.Width / 2 - 20d, StartLocation.Y - 8d, 40d, 16d));
                dc.DrawLine(color, 2.0f, new Point(StartLocation.X + Size.Width / 2 + 20d, StartLocation.Y), EndLocation);
                FormattedText text = new FormattedText(ResistanceString, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10d, new SolidColorBrush(Colors.Black));
                dc.DrawText(ResistanceString, "Arial", 10d, color, new Point(StartLocation.X + Size.Width / 2 - text.Width / 2, StartLocation.Y - 15d - text.Height / 2));
            }
            if (!Horizontal)
            {
                dc.DrawLine(color, 2.0f, StartLocation, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 20d));
                dc.DrawRectangle(Color.FromArgb(0, 255, 255, 255), color, 2d, new Rect(StartLocation.X - 8d, StartLocation.Y + Size.Height / 2 - 20d, 16d, 40d));
                dc.DrawLine(color, 2.0f, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 20d), EndLocation);
                FormattedText text = new FormattedText(ResistanceString, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10d, new SolidColorBrush(Colors.Black));
                dc.DrawText(ResistanceString, "Arial", 10d, color, new Point(StartLocation.X - 15d - text.Width, StartLocation.Y + Size.Height / 2 - text.Height / 2));
            }
        }

        public override void LoadData(System.Xml.XmlReader reader)
        {
            try
            {
                reader.MoveToAttribute("resistance");
                Resistance = reader.ReadContentAsDouble();
            }
            catch (Exception)
            {
            }
        }

        public override void SaveData(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("resistance", Resistance.ToString());
        }

    }
}
