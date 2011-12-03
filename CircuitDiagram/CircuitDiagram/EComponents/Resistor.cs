// Resistor.cs
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
    public class Resistor : EComponent
    {
        [ComponentSerializable(ComponentSerializeOptions.StoreLowercase)]
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

        [ComponentSerializable("t")]
        public ResistorType ResistorType { get; set; }

        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                {
                    if (ResistorType == ResistorType.Potentiometer)
                        return new Rect(new Point(StartLocation.X, StartLocation.Y - 20), new Size(EndLocation.X - StartLocation.X, 50));
                    else if (ResistorType == ResistorType.Variable || ResistorType == EComponents.ResistorType.Thermistor)
                        return new Rect(new Point(StartLocation.X, StartLocation.Y - 20), new Size(EndLocation.X - StartLocation.X, 40));
                    else if (ResistorType == EComponents.ResistorType.LDR)
                        return new Rect(new Point(StartLocation.X, StartLocation.Y - 40), new Size(EndLocation.X - StartLocation.X, 80));
                    else
                        return new Rect(new Point(StartLocation.X, StartLocation.Y - 20), new Size(EndLocation.X - StartLocation.X, 30));
                }
                else
                {
                    if (ResistorType == ResistorType.Potentiometer)
                        return new Rect(new Point(StartLocation.X - 50, StartLocation.Y), new Size(80, EndLocation.Y - StartLocation.Y));
                    else if (ResistorType == ResistorType.Variable || ResistorType == EComponents.ResistorType.Thermistor)
                        return new Rect(new Point(StartLocation.X - 20, StartLocation.Y), new Size(40, EndLocation.Y - StartLocation.Y));
                    else if (ResistorType == EComponents.ResistorType.LDR)
                        return new Rect(new Point(StartLocation.X - 40, StartLocation.Y), new Size(80, EndLocation.Y - StartLocation.Y));
                    else
                        return new Rect(new Point(StartLocation.X - 50, StartLocation.Y), new Size(60, EndLocation.Y - StartLocation.Y));
                }
            }
        }

        public Resistor()
        {
            Resistance = 4700;
            ResistorType = ResistorType.Standard;
            base.Editor = new ResistorEditor(this);
        }

        public override void UpdateLayout()
        {
            this.ImplementMinimumSize(50f);
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (Horizontal)
            {
                Point point0 = new Point(StartLocation.X + Size.Width / 2 - 20d, StartLocation.Y);
                if (ResistorType == EComponents.ResistorType.Potentiometer && point0.X % 10 != 0)
                    point0.X = point0.X + 5d;
                if (ResistorType != EComponents.ResistorType.US)
                    dc.DrawLine(color, 2.0f, StartLocation, point0);
                else
                    dc.DrawLine(color, 2.0f, StartLocation, Point.Add(point0, new Vector(-14d, 0)));
                if (ResistorType != EComponents.ResistorType.US)
                    dc.DrawRectangle(Color.FromArgb(0, 255, 255, 255), color, 2d, new Rect(point0.X, StartLocation.Y - 8d, 40d, 16d));
                else
                    dc.DrawPath(null, color, 2d, String.Format("M {0} l 10,0 l 4,-5 l 8,10 l 8,-10 l 8,10 l 8,-10 l 8,10 l 4,-5 l 10,0", Point.Add(point0, new Vector(-14d, 0))));
                if (ResistorType != EComponents.ResistorType.US)
                    dc.DrawLine(color, 2.0f, new Point(point0.X + 40f, point0.Y), EndLocation);
                else
                    dc.DrawLine(color, 2.0f, new Point(point0.X + 54f, point0.Y), EndLocation);
                FormattedText text = new FormattedText(ResistanceString, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10d, new SolidColorBrush(Colors.Black));
                if (ResistorType != EComponents.ResistorType.Variable && ResistorType != ResistorType.Thermistor && ResistorType != EComponents.ResistorType.LDR)
                    dc.DrawText(ResistanceString, "Arial", 10d, color, new Point(point0.X + 20d - text.Width / 2, StartLocation.Y - 17d - text.Height / 2));

                if (ResistorType == ResistorType.Potentiometer)
                {
                    dc.DrawPath(null, color, 2f, String.Format("M {0} m 14,16 l 6,-6 l 6,6 m -6,-5 l 0,18", point0.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if (ResistorType == ResistorType.Variable)
                {
                    dc.DrawPath(null, color, 2f, String.Format("M {0} m 3,17 l 32,-35 m -6,0 l 6,0 l 0,6", point0.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if (ResistorType == EComponents.ResistorType.Thermistor)
                {
                    dc.DrawPath(null, color, 2f, String.Format("M {0} m -20,17 l 10,0 l 32,-35", new Point(point0.X + 20f, point0.Y).ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if (ResistorType == EComponents.ResistorType.LDR)
                {
                    dc.DrawEllipse(Colors.Transparent, color, 2d, Point.Add(point0, new Vector(20d, 0)), 26d, 26d);
                    dc.DrawPath(color, color, 2.0f, String.Format("M {0} m -16,-26 l 8,8 m 1,1 l -2,-4 l -2,2 l 4,2 l -2,-4", point0.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    dc.DrawPath(color, color, 2.0f, String.Format("M {0} m -10,-32 l 8,8 m 1,1 l -2,-4 l -2,2 l 4,2 l -2,-4", point0.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }
            }
            if (!Horizontal)
            {
                Point point0 = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 20d);
                if (ResistorType == EComponents.ResistorType.Potentiometer && point0.Y % 10 != 0)
                    point0.Y = point0.Y + 5d;
                if (ResistorType != EComponents.ResistorType.US)
                    dc.DrawLine(color, 2.0f, StartLocation, point0);
                else
                    dc.DrawLine(color, 2.0f, StartLocation, Point.Add(point0, new Vector(0, -14d)));
                if (ResistorType != EComponents.ResistorType.US)
                    dc.DrawRectangle(Color.FromArgb(0, 255, 255, 255), color, 2d, new Rect(StartLocation.X - 8d, point0.Y, 16d, 40d));
                else
                    dc.DrawPath(null, color, 2d, String.Format("M {0} l 0,10 l 5,4 l -10,8 l 10,8 l -10,8 l 10,8 l -10,8 l 5,4 l 0,10", Point.Add(point0, new Vector(0, -14d)).ToString(System.Globalization.CultureInfo.InvariantCulture)));
                if (ResistorType != EComponents.ResistorType.US)
                    dc.DrawLine(color, 2.0f, new Point(point0.X, point0.Y + 40f), EndLocation);
                else
                    dc.DrawLine(color, 2.0f, new Point(point0.X, point0.Y + 54f), EndLocation);
                FormattedText text = new FormattedText(ResistanceString, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10d, new SolidColorBrush(Colors.Black));
                if (ResistorType != EComponents.ResistorType.Variable && ResistorType != ResistorType.Thermistor && ResistorType != EComponents.ResistorType.LDR)
                    dc.DrawText(ResistanceString, "Arial", 10d, color, new Point(StartLocation.X - 15d - text.Width, point0.Y + 20d - text.Height / 2));

                if (ResistorType == ResistorType.Potentiometer)
                {
                    dc.DrawPath(null, color, 2f, String.Format("M {0} m 16,14 l -6,6 l 6,6 m -5,-6 l 18,0", point0.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if (ResistorType == ResistorType.Variable)
                {
                    dc.DrawPath(null, color, 2f, String.Format("M {0} m -17,17 l 35,-32 m 0,6 l 0,-6 l -6,0", new Point(point0.X, point0.Y + 20f).ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if (ResistorType == EComponents.ResistorType.Thermistor)
                {
                    dc.DrawPath(null, color, 2f, String.Format("M {0} m -17,-20 l 0,10 l 35,32", new Point(point0.X, point0.Y + 20f).ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if (ResistorType == EComponents.ResistorType.LDR)
                {
                    dc.DrawEllipse(Colors.Transparent, color, 2d, Point.Add(point0, new Vector(0, 20d)), 26d, 26d);
                    dc.DrawPath(color, color, 2.0f, String.Format("M {0} m -26,-16 l 8,8 m 1,1 l -2,-4 l -2,2 l 4,2 l -2,-4", point0.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    dc.DrawPath(color, color, 2.0f, String.Format("M {0} m -32,-10 l 8,8 m 1,1 l -2,-4 l -2,2 l 4,2 l -2,-4", point0.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }
            }
        }
    }

    public enum ResistorType
    {
        Standard = 0,
        Potentiometer = 1,
        LDR = 2,
        Thermistor = 3,
        Variable = 4,
        US = 5
    }
}
