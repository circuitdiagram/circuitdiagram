// Microcontroller.cs
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
    public class Microcontroller : EComponent
    {
        [ComponentSerializable(ComponentSerializeOptions.Lowercase)]
        public bool DisplayPIC { get; set; }
        [ComponentSerializable(ComponentSerializeOptions.Lowercase)]
        public int Inputs { get; set; }
        [ComponentSerializable(ComponentSerializeOptions.Lowercase)]
        public int Outputs { get; set; }
        [ComponentSerializable(ComponentSerializeOptions.Lowercase)]
        public bool ADC { get; set; }

        public override System.Windows.Rect BoundingBox
        {
            get
            {
                int IOCount = Inputs;
                if (Outputs > Inputs)
                    IOCount = Outputs;
                IOCount -= 1;
                if (ADC && Inputs >= Outputs)
                    IOCount = Inputs;
                if (!Horizontal)
                    return new Rect(StartLocation.X - 50f, StartLocation.Y, 100f, 22f + IOCount * 20f);
                else
                    return new Rect(StartLocation.X, StartLocation.Y - 50f, 22f + IOCount * 20f, 100f);
            }
        }

        public Microcontroller()
        {
            this.Editor = new MicrocontrollerEditor(this);
            CanResize = false;
            Inputs = 4;
            Outputs = 4;
            ADC = false;
            DisplayPIC = true;
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (!Horizontal)
            {
                dc.DrawRectangle(Colors.Transparent, color, 2f, new Rect(BoundingBox.X + 10f, BoundingBox.Y, BoundingBox.Width - 20f, BoundingBox.Height));
                for (int i = 0; i < Inputs; i++)
                {
                    dc.DrawLine(color, 2f, new Point(StartLocation.X - 50f, StartLocation.Y + 10f + 20f * i), new Point(StartLocation.X - 40f, StartLocation.Y + 10f + 20f * i));
                    dc.DrawText("I", "Arial", 12f, color, new Point(StartLocation.X - 35f, StartLocation.Y + 10f + 20f * i - 6f));
                    dc.DrawText((Inputs - 1 - i).ToString(), "Arial", 8f, color, new Point(StartLocation.X - 32f, StartLocation.Y + 10f + 20f * i));
                }
                for (int i = 0; i < Outputs; i++)
                {
                    dc.DrawLine(color, 2f, new Point(StartLocation.X + 40f, StartLocation.Y + 10f + 20f * i), new Point(StartLocation.X + 50f, StartLocation.Y + 10f + 20f * i));
                    dc.DrawText("Q", "Arial", 12f, color, new Point(StartLocation.X + 24f, StartLocation.Y + 10f + 20f * i - 6f));
                    dc.DrawText((Outputs - 1 -i).ToString(), "Arial", 8f, color, new Point(StartLocation.X + 33f, StartLocation.Y + 10f + 20f * i));
                }
                if (ADC)
                {
                    dc.DrawLine(color, 2f, new Point(StartLocation.X - 50f, StartLocation.Y + 10f + 20f * Inputs), new Point(StartLocation.X - 40f, StartLocation.Y + 10f + 20f * Inputs));
                    dc.DrawText("adc", "Arial", 12f, color, new Point(StartLocation.X - 35f, StartLocation.Y + 10f + 20f * Inputs - 7f));
                }
                if (DisplayPIC)
                {
                    FormattedText txt = new FormattedText("PIC", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12f, Brushes.Black);
                    dc.DrawText("PIC", "Arial", 12f, color, new Point(StartLocation.X - txt.Width / 2, StartLocation.Y + 4f));
                }
            }
            else
            {
                dc.DrawRectangle(Colors.Transparent, color, 2f, new Rect(BoundingBox.X, BoundingBox.Y + 10f, BoundingBox.Width, BoundingBox.Height - 20f));
                for (int i = 0; i < Inputs; i++)
                {
                    dc.DrawLine(color, 2f, new Point(StartLocation.X + 10f + 20f * i, StartLocation.Y - 50f), new Point(StartLocation.X + 10f + 20f * i, StartLocation.Y - 40f));
                    dc.DrawText("I", "Arial", 12f, color, new Point(StartLocation.X + 14f + 20f * i - 6f, StartLocation.Y - 38f));
                    dc.DrawText((Inputs - 1 - i).ToString(), "Arial", 8f, color, new Point(StartLocation.X + 10f + 20f * i, StartLocation.Y - 32f));
                }
                for (int i = 0; i < Outputs; i++)
                {
                    dc.DrawLine(color, 2f, new Point(StartLocation.X + 10f + 20f * i, StartLocation.Y + 40f), new Point(StartLocation.X + 10f + 20f * i, StartLocation.Y + 50f));
                    dc.DrawText("Q", "Arial", 12f, color, new Point(StartLocation.X + 10f + 20f * i - 6f, StartLocation.Y + 24f));
                    dc.DrawText((Outputs - 1 - i).ToString(), "Arial", 8f, color, new Point(StartLocation.X + 14f + 20f * i, StartLocation.Y + 30f));
                }
                if (ADC)
                {
                    dc.DrawLine(color, 2f, new Point(StartLocation.X + 10f + 20f * Inputs, StartLocation.Y - 50f), new Point(StartLocation.X + 10f + 20f * Inputs, StartLocation.Y - 40f));
                    dc.DrawText("adc", "Arial", 12f, color, new Point(StartLocation.X + 8f + 20f * Inputs - 7f, StartLocation.Y - 38f));
                }
                if (DisplayPIC)
                {
                    dc.DrawText("PIC", "Arial", 12f, color, new Point(StartLocation.X + 4f, StartLocation.Y - 4f));
                }
            }
        }
    }
}
