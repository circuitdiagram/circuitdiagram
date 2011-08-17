// Rail.cs
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
using System.Globalization;

namespace CircuitDiagram.EComponents
{
    public class Rail : EComponent
    {
        private double m_voltage;

        [ComponentSerializable("voltage")]
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
            base.Editor = new RailEditor(this);
        }

        public override void Render(IRenderer dc, Color color)
        {
            FormattedText text = new FormattedText((Voltage > 0 ? "+" : "") + Voltage.ToString() + "V", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12d, new SolidColorBrush(Colors.Black));
            dc.DrawLine(color, 2.0f, StartLocation, EndLocation);
            dc.DrawText((Voltage >0? "+" : "") + Voltage.ToString() + "V", "Arial", 12d, color, Point.Add(StartLocation, new Vector(-(text.Width + 5), -6)));
        }
    }
}
