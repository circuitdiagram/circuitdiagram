using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram.EComponents
{
    public class OpAmp : EComponent
    {
        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 32), new Size(EndLocation.X - StartLocation.X, 64));
                else
                    return new Rect(new Point(StartLocation.X - 32, StartLocation.Y), new Size(64, EndLocation.Y - StartLocation.Y));
            }
        }

        public override double MinimumWidth
        {
            get
            {
                return 60f;
            }
        }

        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public OpAmp()
        {
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (Horizontal)
            {
                Point ref0 = new Point(StartLocation.X + Size.Width / 2 - 20f, StartLocation.Y);
                dc.DrawLine(color, 2.0f, Point.Add(StartLocation, new Vector(0, -10f)), Point.Add(StartLocation, new Vector(ref0.X - StartLocation.X, -10f)));
                dc.DrawLine(color, 2.0f, Point.Add(StartLocation, new Vector(0, 10f)), Point.Add(StartLocation, new Vector(ref0.X - StartLocation.X, 10f)));
                dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " l 0,30 l 44,-30 l -44,-30 l 0,30 m 5,-10 l 10,0 m -5,-5 l 0,10 m -5,15 l 10,0 m 30,-10 L " + EndLocation.ToString());
            }
            else
            {
                Point ref0 = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 20f);
                dc.DrawLine(color, 2.0f, Point.Add(StartLocation, new Vector(-10, 0f)), Point.Add(StartLocation, new Vector(-10f, ref0.Y - StartLocation.Y)));
                dc.DrawLine(color, 2.0f, Point.Add(StartLocation, new Vector(10, 0f)), Point.Add(StartLocation, new Vector(10f, ref0.Y - StartLocation.Y)));
                dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " l 30,0 l -30,44 l -30,-44 l 30,0 m -10,5 l 0,10 m -5,-5 l 10,0 m 15,-5 l 0,10 m -10,30 L " + EndLocation.ToString());

            }
        }

        public override void LoadData(System.Xml.XmlReader reader)
        {
        }

        public override void SaveData(System.Xml.XmlWriter writer)
        {
        }
    }
}
