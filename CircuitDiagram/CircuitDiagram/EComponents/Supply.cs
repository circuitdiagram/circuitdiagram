// Supply.cs
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
    public class Supply : EComponent
    {
        const double Gap = 6.0d;
        const double PlusTerminalLength = 20d;
        const double MinusTerminalLength = 10d;

        public Supply()
        {
            CanFlip = true;
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
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 10), new Size(EndLocation.X - StartLocation.X, 20));
                else
                    return new Rect(new Point(StartLocation.X - 10, StartLocation.Y), new Size(20, EndLocation.Y - StartLocation.Y));
            }
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (Horizontal)
            {
                dc.DrawLine(color, 2.0f, StartLocation, new Point(StartLocation.X + Size.Width / 2 - 0.5 * Gap, StartLocation.Y));
                if (!IsFlipped)
                {
                    dc.DrawLine(color, 2.0f, new Point(StartLocation.X + Size.Width / 2 - 0.5 * Gap, StartLocation.Y - 0.5 * PlusTerminalLength), new Point(StartLocation.X + Size.Width / 2 - 0.5 * Gap, StartLocation.Y + 0.5 * PlusTerminalLength));
                    dc.DrawLine(color, 3.0f, new Point(StartLocation.X + Size.Width / 2 + 0.5 * Gap, StartLocation.Y - 0.5 * MinusTerminalLength), new Point(StartLocation.X + Size.Width / 2 + 0.5 * Gap, StartLocation.Y + 0.5 * MinusTerminalLength));
                }
                else
                {
                    dc.DrawLine(color, 3.0f, new Point(StartLocation.X + Size.Width / 2 - 0.5 * Gap, StartLocation.Y - 0.5 * MinusTerminalLength), new Point(StartLocation.X + Size.Width / 2 - 0.5 * Gap, StartLocation.Y + 0.5 * MinusTerminalLength));
                    dc.DrawLine(color, 2.0f, new Point(StartLocation.X + Size.Width / 2 + 0.5 * Gap, StartLocation.Y - 0.5 * PlusTerminalLength), new Point(StartLocation.X + Size.Width / 2 + 0.5 * Gap, StartLocation.Y + 0.5 * PlusTerminalLength));
                }
                dc.DrawLine(color, 2.0f, new Point(StartLocation.X + Size.Width / 2 + 0.5 * Gap, EndLocation.Y), EndLocation);
            }
            else
            {
                dc.DrawLine(color, 2.0f, StartLocation, new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 0.5 * Gap));
                if (!IsFlipped)
                {
                    dc.DrawLine(color, 2.0f, new Point(StartLocation.X - 0.5 * PlusTerminalLength, StartLocation.Y + Size.Height / 2 - 0.5 * Gap), new Point(StartLocation.X + 0.5 * PlusTerminalLength, StartLocation.Y + Size.Height / 2 - 0.5 * Gap));
                    dc.DrawLine(color, 3.0f, new Point(StartLocation.X - 0.5 * MinusTerminalLength, StartLocation.Y + Size.Height / 2 + 0.5 * Gap), new Point(StartLocation.X + 0.5 * MinusTerminalLength, StartLocation.Y + Size.Height / 2 + 0.5 * Gap));
                }
                else
                {
                    dc.DrawLine(color, 3.0f, new Point(StartLocation.X - 0.5 * MinusTerminalLength, StartLocation.Y + Size.Height / 2 - 0.5 * Gap), new Point(StartLocation.X + 0.5 * MinusTerminalLength, StartLocation.Y + Size.Height / 2 - 0.5 * Gap));
                    dc.DrawLine(color, 2.0f, new Point(StartLocation.X - 0.5 * PlusTerminalLength, StartLocation.Y + Size.Height / 2 + 0.5 * Gap), new Point(StartLocation.X + 0.5 * PlusTerminalLength, StartLocation.Y + Size.Height / 2 + 0.5 * Gap));
                }
                dc.DrawLine(color, 2.0f, new Point(EndLocation.X, EndLocation.Y - Size.Height / 2 + 0.5 * Gap), EndLocation);
            }
        }
    }
}