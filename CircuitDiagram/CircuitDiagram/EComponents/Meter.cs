// Meter.cs
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
    public class Meter : EComponent
    {
        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        [ComponentSerializable("t")]
        public MeterType Type { get; set; }

        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 14), new Size(EndLocation.X - StartLocation.X, 28));
                else
                    return new Rect(new Point(StartLocation.X - 14, StartLocation.Y), new Size(28, EndLocation.Y - StartLocation.Y));
            }
        }

        public Meter()
        {
            Type = MeterType.Voltmeter;
            this.Editor = new MeterEditor(this);
        }

        public override void UpdateLayout()
        {
            this.ImplementMinimumSize(30f);
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (Horizontal)
            {
                dc.DrawLine(color, 2.0f, StartLocation, new Point(StartLocation.X + Size.Width / 2 - 12d, StartLocation.Y));
                dc.DrawEllipse(Colors.Transparent, color, 2d, new Point(StartLocation.X + Size.Width / 2, StartLocation.Y), 12d, 12d);
                dc.DrawLine(color, 2.0f, new Point(StartLocation.X + Size.Width / 2 + 12d, StartLocation.Y), EndLocation);
                if (Type == MeterType.Voltmeter)
                {
                    FormattedText text = new FormattedText("V", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(new FontFamily("Calibri"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal), 18d, new SolidColorBrush(color));
                    dc.DrawFormattedText(text, new Point(StartLocation.X + Size.Width / 2 - text.Width / 2, StartLocation.Y - text.Height / 2));
                }
                else if (Type == MeterType.Ammeter)
                {
                    FormattedText text = new FormattedText("A", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(new FontFamily("Calibri"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal), 18d, new SolidColorBrush(color));
                    dc.DrawFormattedText(text, new Point(StartLocation.X + Size.Width / 2 - text.Width / 2, StartLocation.Y - text.Height / 2));
                }
                else if (Type == MeterType.Oscilloscope)
                {
                }
            }
            else
            {
                dc.DrawLine(color, 2.0f, StartLocation, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 12d));
                dc.DrawEllipse(Colors.Transparent, color, 2d, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2), 12d, 12d);
                dc.DrawLine(color, 2.0f, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 12d), EndLocation);
                if (Type == MeterType.Voltmeter)
                {
                    FormattedText text = new FormattedText("V", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(new FontFamily("Calibri"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal), 18d, new SolidColorBrush(color));
                    dc.DrawFormattedText(text, new Point(StartLocation.X - text.Width / 2, StartLocation.Y + Size.Height / 2 - text.Height / 2));
                }
                else if (Type == MeterType.Ammeter)
                {
                    FormattedText text = new FormattedText("A", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(new FontFamily("Calibri"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal), 18d, new SolidColorBrush(color));
                    dc.DrawFormattedText(text, new Point(StartLocation.X - text.Width / 2, StartLocation.Y + Size.Height / 2 - text.Height / 2));
                }
                else if (Type == MeterType.Oscilloscope)
                {
                }
            }
        }

        public enum MeterType
        {
            Voltmeter,
            Ammeter,
            Oscilloscope
        }
    }
}
