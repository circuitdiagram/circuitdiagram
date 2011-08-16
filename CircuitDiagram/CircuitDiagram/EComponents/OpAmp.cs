// OpAmp.cs
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

        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public bool FlipInputs { get; set; }

        public OpAmp()
        {
            FlipInputs = false;
            this.Editor = new OpAmpEditor(this);
        }

        protected override void CustomUpdateLayout()
        {
            ImplementMinimumSize(60f);
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (Horizontal)
            {
                Point ref0 = new Point(StartLocation.X + Size.Width / 2 - 20f, StartLocation.Y);
                dc.DrawLine(color, 2.0f, Point.Add(StartLocation, new Vector(0, -10f)), Point.Add(StartLocation, new Vector(ref0.X - StartLocation.X, -10f)));
                dc.DrawLine(color, 2.0f, Point.Add(StartLocation, new Vector(0, 10f)), Point.Add(StartLocation, new Vector(ref0.X - StartLocation.X, 10f)));
                if (FlipInputs)
                    dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " l 0,30 l 44,-30 l -44,-30 l 0,30 m 5,-10 l 10,0 m -5,-5 l 0,10 m -5,15 l 10,0 m 28,-10 L " + EndLocation.ToString());
                else
                    dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " l 0,30 l 44,-30 l -44,-30 l 0,30 m 5,-10 l 10,0 m -5,15 l 0,10 m -5,-5 l 10,0 m 28,-10 L " + EndLocation.ToString());
            }
            else
            {
                Point ref0 = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 20f);
                dc.DrawLine(color, 2.0f, Point.Add(StartLocation, new Vector(-10, 0f)), Point.Add(StartLocation, new Vector(-10f, ref0.Y - StartLocation.Y)));
                dc.DrawLine(color, 2.0f, Point.Add(StartLocation, new Vector(10, 0f)), Point.Add(StartLocation, new Vector(10f, ref0.Y - StartLocation.Y)));
                if (!FlipInputs)
                    dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " l 30,0 l -30,44 l -30,-44 l 30,0 m -10,5 l 0,10 m -5,-5 l 10,0 m 15,-5 l 0,10 m -10,28 L " + EndLocation.ToString());
                else
                    dc.DrawPath(null, color, 2f, "M " + ref0.ToString() + " l 30,0 l -30,44 l -30,-44 l 30,0 m -10,5 l 0,10 m 15,-5 l 10,0 m -5,-5 l 0,10 m -10,28 L " + EndLocation.ToString());
            }
        }

        public override void LoadData(System.Xml.XmlReader reader)
        {
            try
            {
                reader.MoveToAttribute("flipinputs");
                string flipInputs = reader.ReadContentAsString();
                if (flipInputs.ToLower() == "true")
                    FlipInputs = true;
                else
                    FlipInputs = false;
            }
            catch (Exception)
            {
            }
        }

        public override void SaveData(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("flipinputs", FlipInputs.ToString());
        }

        public override void SaveData(System.IO.TextWriter writer)
        {
            base.SaveData(writer);
            EComponent.WriteProperty(writer, "flipinputs", FlipInputs.ToString().ToLower());
        }

        public override void LoadData(System.IO.TextReader reader)
        {
            Dictionary<string, string> properties;
            base.LoadData(reader, out properties);
            FlipInputs = false;
            if (properties.ContainsKey("flipinputs") && properties["flipinputs"] == "true")
                FlipInputs = true;
        }
    }
}
