using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace CircuitDiagram.EComponents
{
    public class Rail : EComponent
    {
        private double m_voltage;

        public double Voltage
        {
            get { return m_voltage; }
            set { m_voltage = value; }
        }

        public override Rect BoundingBox
        {
            get
            {
                FormattedText text = new FormattedText((Voltage > 0 ? "+" : "") + Voltage.ToString() + "V", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12d, new SolidColorBrush(Colors.Black));
                if (Horizontal)
                    return new Rect(new Point(StartLocation.X - (text.Width + 5), StartLocation.Y - 6), new Size(EndLocation.X - StartLocation.X + text.Width + 5, 12));
                else
                    return new Rect(new Point(StartLocation.X - (text.Width + 5), StartLocation.Y - 5), new Size((text.Width + 9), EndLocation.Y - StartLocation.Y + 5));
            }
        }

        public Rail()
        {
            m_voltage = 5d;
            base.Editor = new RailEditor();
        }

        public override bool Intersects(Point point)
        {
            Rect thisRect = new Rect(StartLocation, EndLocation - StartLocation);
            return thisRect.IntersectsWith(new Rect(point, new Size(1,1)));
        }

        public override void Render(IRenderer dc, Color color)
        {
            FormattedText text = new FormattedText((Voltage > 0 ? "+" : "") + Voltage.ToString() + "V", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12d, new SolidColorBrush(Colors.Black));
            dc.DrawLine(color, 2.0f, StartLocation, EndLocation);
            dc.DrawText((Voltage >0? "+" : "") + Voltage.ToString() + "V", "Arial", 12d, color, Point.Add(StartLocation, new Vector(-(text.Width + 5), -6)));
        }

        public override void LoadData(System.Xml.XmlReader reader)
        {
            try
            {
                reader.MoveToAttribute("voltage");
                Voltage = reader.ReadContentAsDouble();
            }
            catch (Exception)
            {
            }
        }

        public override void SaveData(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("voltage", Voltage.ToString());
        }
    }
}
