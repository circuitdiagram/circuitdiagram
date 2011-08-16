// Switch.cs
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
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram.EComponents
{
    public class Switch : EComponent
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
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 15), new Size(EndLocation.X - StartLocation.X, 18));
                else
                    return new Rect(new Point(StartLocation.X - 15, StartLocation.Y), new Size(18, EndLocation.Y - StartLocation.Y));
            }
        }

        public SwitchType Type
        {
            get;
            set;
        }

        public Switch()
        {
            Type = SwitchType.Push;
            this.Editor = new SwitchEditor(this);
        }

        protected override void CustomUpdateLayout()
        {
            ImplementMinimumSize(30d);
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (Horizontal)
            {
                Point gapStart = new Point(StartLocation.X + Size.Width / 2 - 12, StartLocation.Y);
                Point gapEnd = new Point(StartLocation.X + Size.Width / 2 + 12, StartLocation.Y);
                dc.DrawLine(color, 2d, StartLocation, gapStart);
                dc.DrawEllipse(Colors.White, color, 2d, gapStart, 3d, 3d);
                dc.DrawLine(color, 2d, gapEnd, EndLocation);
                dc.DrawEllipse(Colors.White, color, 2d, gapEnd, 3d, 3d);
                if (Type == SwitchType.Push)
                {
                    dc.DrawPath(null, color, 2f, "M " + gapStart.ToString() + " m -2,-8 l 28,0 m -14,0 l 0,-6 m -6,0 l 12,0");
                }
                else if (Type == SwitchType.Toggle)
                {
                    dc.DrawLine(color, 2d, Point.Add(gapStart, new Vector(3d, -1d)), Point.Add(gapEnd, new Vector(0, -8d)));
                }
            }
            if (!Horizontal)
            {
                Point gapStart = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 12);
                Point gapEnd = new Point(StartLocation.X, StartLocation.Y+ Size.Height / 2 + 12);
                dc.DrawPath(null, color, 2f, "M " + StartLocation.ToString() + " L " + gapStart.ToString() + " m 0,24 L" + EndLocation.ToString());
                dc.DrawEllipse(Colors.White, color, 2d, gapStart, 3d, 3d);
                dc.DrawEllipse(Colors.White, color, 2d, Point.Add(gapStart, new Vector(0, 24d)), 3d, 3d);
                if (Type == SwitchType.Push)
                {
                    dc.DrawPath(null, color, 2f, "M " + gapStart.ToString() + " m -8,-2 l 0,28 m 0,-14 l -6,0 m 0,-6 l 0,12");
                }
                else if (Type == SwitchType.Toggle)
                {
                    dc.DrawLine(color, 2d, Point.Add(gapStart, new Vector(-1d, 3d)), Point.Add(gapEnd, new Vector(-8d, 0)));
                }
            }
        }

        public override void LoadData(System.Xml.XmlReader reader)
        {
            Type = SwitchType.Push;

            try
            {
                string switchType = reader.GetAttribute("t");
                if (switchType != null)
                    Type = (SwitchType)int.Parse(switchType);
            }
            catch (Exception)
            {
            }
        }

        public override void SaveData(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("t", ((int)Type).ToString());
        }

        public override void LoadData(System.IO.TextReader reader)
        {
            Dictionary<string, string> properties;
            base.LoadData(reader, out properties);
            if (properties.ContainsKey("t"))
                Type = (SwitchType)int.Parse(properties["t"]);
        }

        public override void SaveData(System.IO.TextWriter writer)
        {
            base.SaveData(writer);
            writer.WriteLine("t:{0}", (int)Type);
        }
    }

    public enum SwitchType
    {
        Push,
        Toggle
    }
}
