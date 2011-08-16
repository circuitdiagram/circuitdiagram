// OutputDevice.cs
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
    public class OutputDevice : EComponent
    {
        public OutputDeviceType Type { get; set; }

        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                {
                    if (Type == OutputDeviceType.Loudspeaker || Type == OutputDeviceType.Buzzer)
                        return new Rect(Point.Add(StartLocation, new Vector(0d, -10d)), Point.Add(EndLocation, new Vector(0d, 26d)));
                    else if (Type == OutputDeviceType.Heater || Type == OutputDeviceType.Motor)
                        return new Rect(Point.Add(StartLocation, new Vector(0d, -10d)), Point.Add(EndLocation, new Vector(0d, 10d)));
                    else return base.BoundingBox;
                }
                else
                {
                    if (Type == OutputDeviceType.Loudspeaker || Type == OutputDeviceType.Buzzer)
                        return new Rect(Point.Add(StartLocation, new Vector(-10d, 0d)), Point.Add(EndLocation, new Vector(26d, 0d)));
                    else if (Type == OutputDeviceType.Heater || Type == OutputDeviceType.Motor)
                        return new Rect(Point.Add(StartLocation, new Vector(-10d, 0d)), Point.Add(EndLocation, new Vector(10d, 0d)));
                    else return base.BoundingBox;
                }
            }
        }

        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public OutputDevice()
        {
            base.Editor = new OutputDeviceEditor(this);
            Type = OutputDeviceType.Loudspeaker;
        }

        protected override void CustomUpdateLayout()
        {
            if (Type == OutputDeviceType.Heater)
                ImplementMinimumSize(80d);
            else
                ImplementMinimumSize(70f);
        }

        public override void Render(IRenderer dc, Color colour)
        {
            if (Horizontal)
            {
                if (Type == OutputDeviceType.Loudspeaker)
                {
                    Point start = new Point(StartLocation.X + Size.Width / 2 - 20d, StartLocation.Y);
                    Point end = new Point(StartLocation.X + Size.Width / 2 + 20d, StartLocation.Y);
                    dc.DrawLine(colour, 2d, StartLocation, start);
                    dc.DrawLine(colour, 2d, end, EndLocation);
                    dc.DrawPath(null, colour, 2d, String.Format("M {0} l 0,8 l -15,15 l 70,0 l -15,-15 l -40,0 l 0,-16 l 40,0 l 0,16", start));
                }
                else if (Type == OutputDeviceType.Buzzer)
                {
                    Point start = new Point(StartLocation.X + Size.Width / 2 - 20d, StartLocation.Y);
                    Point end = new Point(StartLocation.X + Size.Width / 2 + 20d, StartLocation.Y);
                    dc.DrawLine(colour, 2d, StartLocation, new Point(start.X, start.Y));
                    dc.DrawPath(null, colour, 2d, String.Format("M {0} l 6,0 l 0,10", start));
                    dc.DrawLine(colour, 2d, end, EndLocation);
                    dc.DrawPath(null, colour, 2d, String.Format("M {0} l -6,0 l 0,10", end));
                    dc.DrawPath(Colors.White, colour, 2d, String.Format("M {0} a 20,20 90 0 0 -20,20 l 40,0 a 20,20 90 0 0 -20,-20", new Point(start.X + 20d, start.Y)));
                }
                else if (Type == OutputDeviceType.Heater)
                {
                    Point start = new Point(StartLocation.X + Size.Width / 2 - 40d, StartLocation.Y);
                    Point end = new Point(StartLocation.X + Size.Width / 2 + 40d, StartLocation.Y);
                    dc.DrawLine(colour, 2d, StartLocation, start);
                    dc.DrawLine(colour, 2d, end, EndLocation);
                    dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(new Point(start.X, start.Y - 10d), new Size(20d, 20d)));
                    dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(new Point(start.X + 20d, start.Y - 10d), new Size(20d, 20d)));
                    dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(new Point(start.X + 40d, start.Y - 10d), new Size(20d, 20d)));
                    dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(new Point(start.X + 60d, start.Y - 10d), new Size(20d, 20d)));
                }
                else if (Type == OutputDeviceType.Motor)
                {
                    dc.DrawLine(colour, 2.0f, StartLocation, new Point(StartLocation.X + Size.Width / 2 - 12d, StartLocation.Y));
                    dc.DrawEllipse(Colors.Transparent, colour, 2d, new Point(StartLocation.X + Size.Width / 2, StartLocation.Y), 12d, 12d);
                    dc.DrawLine(colour, 2.0f, new Point(StartLocation.X + Size.Width / 2 + 12d, StartLocation.Y), EndLocation);
                    FormattedText text = new FormattedText("M", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(new FontFamily("Calibri"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal), 18d, new SolidColorBrush(colour));
                    dc.DrawFormattedText(text, new Point(StartLocation.X + Size.Width / 2 - text.Width / 2, StartLocation.Y - text.Height / 2));
                }
            }
            else
            {
                if (Type == OutputDeviceType.Loudspeaker)
                {
                    Point start = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 20d);
                    Point end = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 20d);
                    dc.DrawLine(colour, 2d, StartLocation, start);
                    dc.DrawLine(colour, 2d, end, EndLocation);
                    dc.DrawPath(null, colour, 2d, String.Format("M {0} l 8,0 l 15,-15 l 0,70 l -15,-15 l 0,-40 l -16,0 l 0,40 l 16,0", start));
                }
                else if (Type == OutputDeviceType.Buzzer)
                {
                    Point start = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 20d);
                    Point end = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 20d);
                    dc.DrawLine(colour, 2d, StartLocation, new Point(start.X, start.Y));
                    dc.DrawPath(null, colour, 2d, String.Format("M {0} l 0,6 l 10,0", start));
                    dc.DrawLine(colour, 2d, end, EndLocation);
                    dc.DrawPath(null, colour, 2d, String.Format("M {0} l 0,-6 l 10,0", end));
                    dc.DrawPath(Colors.White, colour, 2d, String.Format("M {0} a 20,20 90 0 1 20,-20 l 0,40 a 20,20 90 0 1 -20,-20", new Point(start.X, start.Y + 20d)));
                }
                else if (Type == OutputDeviceType.Heater)
                {
                    Point start = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 40d);
                    Point end = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 40d);
                    dc.DrawLine(colour, 2d, StartLocation, start);
                    dc.DrawLine(colour, 2d, end, EndLocation);
                    dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(new Point(start.X - 10d, start.Y), new Size(20d, 20d)));
                    dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(new Point(start.X - 10d, start.Y + 20d), new Size(20d, 20d)));
                    dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(new Point(start.X - 10d, start.Y + 40d), new Size(20d, 20d)));
                    dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(new Point(start.X - 10d, start.Y + 60d), new Size(20d, 20d)));
                }
                else if (Type == OutputDeviceType.Motor)
                {
                    dc.DrawLine(colour, 2.0f, StartLocation, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 12d));
                    dc.DrawEllipse(Colors.Transparent, colour, 2d, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2), 12d, 12d);
                    dc.DrawLine(colour, 2.0f, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 12d), EndLocation);
                    FormattedText text = new FormattedText("M", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(new FontFamily("Calibri"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal), 18d, new SolidColorBrush(colour));
                    dc.DrawFormattedText(text, new Point(StartLocation.X - text.Width / 2, StartLocation.Y + Size.Height / 2 - text.Height / 2 - 2));
                }
            }
        }

        public override void LoadData(System.Xml.XmlReader reader)
        {
            try
            {
                reader.MoveToAttribute("t");
                Type = (OutputDeviceType)reader.ReadContentAsInt();
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
                Type = (OutputDeviceType)int.Parse(properties["t"]);
        }

        public override void SaveData(System.IO.TextWriter writer)
        {
            base.SaveData(writer);
            EComponent.WriteProperty(writer, "t", ((int)Type).ToString());
        }
    }

    public enum OutputDeviceType
    {
        Loudspeaker = 0,
        Motor = 1,
        Buzzer = 2,
        Heater = 3
    }
}
