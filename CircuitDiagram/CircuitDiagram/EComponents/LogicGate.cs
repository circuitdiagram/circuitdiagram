using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CircuitDiagram.EComponents
{
    public class LogicGate : EComponent
    {
        private LogicType m_logicType;

        public LogicType LogicType
        {
            get { return m_logicType; }
            set { m_logicType = value; }
        }

        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public LogicGate()
        {
            m_logicType = LogicType.AND;
            base.Editor = new LogicGateEditor();
        }

        public override bool Intersects(Point point)
        {
            Rect thisRect = new Rect(StartLocation, EndLocation - StartLocation);
            Rect thisRect2 = new Rect(new Point(StartLocation.X + Size.Width / 2 - 20f, StartLocation.Y - 10f), new Point(StartLocation.X + Size.Width / 2 + 20f, StartLocation.Y + 10f));
            return thisRect.IntersectsWith(new Rect(point, new Size(1, 1))) || thisRect2.IntersectsWith(new Rect(point, new Size(1,1)));
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (Horizontal)
            {
                Point ref0 = new Point(StartLocation.X + Size.Width / 2 - 20f, StartLocation.Y - 10f);
                Point ref1;

                switch (LogicType)
                {
                    case LogicType.AND:
                        dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " m 0,-10 L " + ref0.ToString() + " m 0,20 L " + Point.Add(StartLocation, new Vector(0f, 10f)).ToString());
                        dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " m 0,-10 l 0,40 l 24,0 m -25,-40 l 24,0 m 0,40 a 5,5 90 1 0 0,-40 m 20,20 L " + EndLocation.ToString());
                        break;
                    case LogicType.NAND:
                        dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " m 0,-10 L " + ref0.ToString() + " m 0,20 L " + Point.Add(StartLocation, new Vector(0f, 10f)).ToString());
                        dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " m 0,-10 l 0,40 l 24,0 m -25,-40 l 24,0 m 0,40 a 5,5 90 1 0 0,-40 m 21,20 a 2,2 -5 0,1 6,0 a 2,2 -5 0,1 -6,0 m 6,0 L " + EndLocation.ToString());
                        break;
                    case LogicType.OR:
                        ref1 = Point.Add(ref0, new Vector(7, 0));
                        dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " m 0,-10 L " + ref1.ToString() + " m 0,20 L " + Point.Add(StartLocation, new Vector(0f, 10f)).ToString());
                        dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " m 0,-10 q 20,20 0,40 m -1,0 q 35,-5 50,-20 q -15,-15 -50,-20 m 50,20 L " + EndLocation.ToString());
                        break;
                    case LogicType.NOR:
                        ref1 = Point.Add(ref0, new Vector(7, 0));
                        dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " m 0,-10 L " + ref1.ToString() + " m 0,20 L " + Point.Add(StartLocation, new Vector(0f, 10f)).ToString());
                        dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " m 0,-10 q 20,20 0,40 m -1,0 q 35,-5 50,-20 q -15,-15 -50,-20 m 51,20 a 2,2 -5 0,1 6,0 a 2,2 -5 0,1 -6,0 m 6,0 L " + EndLocation.ToString());
                        break;
                }
            }
            else
            {
                Point ref0 = new Point(StartLocation.X - 10f, StartLocation.Y + Size.Height / 2 - 20f);

                switch (LogicType)
                {
                    case LogicType.AND:
                        dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " m -10,0 L " + ref0.ToString() + " m 20,0 L " + Point.Add(StartLocation, new Vector(10f, 0f)).ToString());
                        dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " m -10,0 l 40,0 l 0,24 m -40,-25 l 0,24 a 5,5 90 1 0 40,0 m -20,20 L " + EndLocation.ToString());
                        break;
                }
            }
        }

        public override void LoadData(System.Xml.XmlReader reader)
        {
            try
            {
                reader.MoveToAttribute("type");
                LogicType = (LogicType)reader.ReadContentAsInt();
            }
            catch (Exception)
            {
            }
        }

        public override void SaveData(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("type", ((int)LogicType).ToString());
        }
    }

    public enum LogicType
    {
        AND,
        NAND,
        OR,
        NOR,
        NOT
    }
}
