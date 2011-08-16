// LogicGate.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2011  Sam Fisher
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

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

        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 20), new Size(EndLocation.X - StartLocation.X, 40));
                else
                    return new Rect(new Point(StartLocation.X - 20, StartLocation.Y), new Size(40, EndLocation.Y - StartLocation.Y));
            }
        }

        public LogicGate()
        {
            m_logicType = LogicType.AND;
            base.Editor = new LogicGateEditor(this);
        }

        protected override void CustomUpdateLayout()
        {
            ImplementMinimumSize(64f);
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
                    case LogicType.XOR:
                        ref1 = Point.Add(ref0, new Vector(15, 0));
                        dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " m 0,-10 L " + ref1.ToString() + " m 0,20 L " + Point.Add(StartLocation, new Vector(0f, 10f)).ToString());
                        dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " m 0,-10 q 20,20 0,40 m 8,-40 q 20,20 0,40 m -1,0 q 35,-5 50,-20 q -15,-15 -50,-20 m 50,20 L " + EndLocation.ToString());
                        break;
                    case LogicType.NOT:
                        dc.DrawPath(null, color, 2f, "M" + StartLocation.ToString() + " L " + new Point(ref0.X, ref0.Y + 10f).ToString() + " M " + ref0.ToString() + " m 0,10 l 0,18 l 28,-18 l -28,-18 l 0,18 m 28,0 a 2,2 -5 0,1 6,0 a 2,2 -5 0,1 -6,0 m 6,0 L " + EndLocation.ToString());
                        break;
                    case LogicType.Schmitt:
                        dc.DrawPath(null, color, 2f, "M" + StartLocation.ToString() + " L " + new Point(ref0.X, ref0.Y + 10f).ToString() + " M " + ref0.ToString() + " m 0,10 l 0,18 l 28,-18 l -28,-18 l 0,18 m 3,2.5 l 10,0 l 0,-5 m 5,0 l -10,0 l 0,5 m 0,-2.5 m 20,0 a 2,2 -5 0,1 6,0 a 2,2 -5 0,1 -6,0 m 6,0 L " + EndLocation.ToString());
                        break;
                }
            }
            else
            {
                Point ref0 = new Point(StartLocation.X - 10f, StartLocation.Y + Size.Height / 2 - 20f);
                Point ref1;

                switch (LogicType)
                {
                    case LogicType.AND:
                        dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " m -10,0 L " + ref0.ToString() + " m 20,0 L " + Point.Add(StartLocation, new Vector(10f, 0f)).ToString());
                        dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " m -10,0 l 40,0 l 0,24 m -40,-25 l 0,24 a 5,5 90 1 0 40,0 m -20,20 L " + EndLocation.ToString());
                        break;
                    case LogicType.NAND:
                        dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " m -10,0 L " + ref0.ToString() + " m 20,0 L " + Point.Add(StartLocation, new Vector(10f, 0f)).ToString());
                        dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " m -10,0 l 40,0 l 0,24 m -40,-25 l 0,24 a 5,5 90 1 0 40,0 m -20,20 a 2,2 5 1,0 0,6 a 2,2 5 1,0 0,-6 m 0,6 L " + EndLocation.ToString());
                        break;
                    case LogicType.OR:
                        ref1 = Point.Add(ref0, new Vector(0, 7));
                        dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " m -10,0 L " + ref1.ToString() + " m 20,0 L " + Point.Add(StartLocation, new Vector(10f, 0f)).ToString());
                        dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " m -10,0 q 20,20 40,0 m 0,-1 q -5,35 -20,50 q -15,-15 -20,-50 m 20,50 L " + EndLocation.ToString());
                        break;
                    case LogicType.NOR:
                        ref1 = Point.Add(ref0, new Vector(0, 7));
                        dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " m -10,0 L " + ref1.ToString() + " m 20,0 L " + Point.Add(StartLocation, new Vector(10f, 0f)).ToString());
                        dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " m -10,0 q 20,20 40,0 m 0,-1 q -5,35 -20,50 q -15,-15 -20,-50 m 20,51 a 2,2 5 1,0 0,6 a 2,2 5 1,0 0,-6 m 0,6 L " + EndLocation.ToString());
                        break;
                    case LogicType.XOR:
                        ref1 = Point.Add(ref0, new Vector(0, 15));
                        dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " m -10,0 L " + ref1.ToString() + " m 20,0 L " + Point.Add(StartLocation, new Vector(10f, 0f)).ToString());
                        dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " m -10,0 q 20,20 40,0 m -40,8 q 20,20 40,0 m 0,-1 q -5,35 -20,50 q -15,-15 -20,-50 m 20,50 L " + EndLocation.ToString());
                        break;
                    case LogicType.NOT:
                        dc.DrawPath(null, color, 2f, "M" + StartLocation.ToString() + " L " + new Point(ref0.X + 10f, ref0.Y).ToString() + " M " + ref0.ToString() + " m 10,0 l 18,0 l -18,28 l -18,-28 l 18,0 m 0,28 a 2,2 5 1,0 0,6 a 2,2 5 1,0 0,-6 m 0,6 L " + EndLocation.ToString());
                        break;
                    case LogicType.Schmitt:
                        dc.DrawPath(null, color, 2f, "M" + StartLocation.ToString() + " L " + new Point(ref0.X + 10f, ref0.Y).ToString() + " M " + ref0.ToString() + " m 10,0 l 18,0 l -18,28 l -18,-28 l 18,0 m 2.5,3 l 0,10 l -5,0 m 0,5 l 0,-10 l 5,0 m -2.5,0 m 0,20 a 2,2 5 1,0 0,6 a 2,2 5 1,0 0,-6 m 0,6 L " + EndLocation.ToString());
                        break;
                }
            }
        }

        public override void LoadData(System.Xml.XmlReader reader)
        {
            try
            {
                reader.MoveToAttribute("logictype");
                LogicType = (LogicType)reader.ReadContentAsInt();
            }
            catch (Exception)
            {
            }
        }

        public override void SaveData(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("logictype", ((int)LogicType).ToString());
        }

        public override void SaveData(System.IO.TextWriter writer)
        {
            base.SaveData(writer);
            EComponent.WriteProperty(writer, "t", ((int)LogicType).ToString());
        }

        public override void LoadData(System.IO.TextReader reader)
        {
            Dictionary<string, string> properties;
            base.LoadData(reader, out properties);
            if (properties.ContainsKey("t"))
                LogicType = (LogicType)int.Parse(properties["t"]);
        }
    }

    public enum LogicType
    {
        AND,
        NAND,
        OR,
        NOR,
        XOR,
        NOT,
        Schmitt
    }
}
