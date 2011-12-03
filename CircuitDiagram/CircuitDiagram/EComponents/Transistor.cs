// Transistor.cs
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
    public class Transistor : EComponent
    {
        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 30), new Size(EndLocation.X - StartLocation.X, 60));
                else
                    return new Rect(new Point(StartLocation.X - 40, StartLocation.Y), new Size(54, EndLocation.Y - StartLocation.Y));
            }
        }

        [ComponentSerializable("t")]
        public TransistorType Type
        {
            get;
            set;
        }

        public Transistor()
        {
            Type = TransistorType.NPN;
            this.Editor = new TransistorEditor(this);
        }

        public override void UpdateLayout()
        {
            this.ImplementMinimumSize(64f);
        }

        public override void Render(IRenderer dc, Color color)
        {
            if (!Horizontal)
            {
                Point startPos = Point.Add(StartLocation, new Vector((EndLocation.X - StartLocation.X) / 2, (EndLocation.Y - StartLocation.Y) / 2 - 30f));
                if (startPos.Y % 10 != 0)
                    startPos.Y = startPos.Y + 5d;
                if (Type == TransistorType.NPN)
                {
                    startPos = Point.Add(StartLocation, new Vector(-10d, (EndLocation.Y - StartLocation.Y) / 2));
                    if (startPos.Y % 10 != 0)
                        startPos.Y = startPos.Y + 5d;
                    dc.DrawLine(color, 2d, StartLocation, EndLocation);
                    dc.DrawEllipse(Colors.White, color, 2d, startPos, 16d, 16d);
                    dc.DrawLine(color, 3d, new Point(startPos.X - 6d, startPos.Y - 10d), new Point(startPos.X - 6d, startPos.Y + 10d));
                    dc.DrawLine(color, 2d, new Point(startPos.X - 30d, startPos.Y), new Point(startPos.X - 6d, startPos.Y));
                    dc.DrawLine(color, 2d, new Point(StartLocation.X + 1d, startPos.Y - 13d), new Point(startPos.X - 6d, startPos.Y - 4d));
                    dc.DrawLine(color, 2d, new Point(StartLocation.X + 1d, startPos.Y + 13d), new Point(startPos.X - 6d, startPos.Y + 4d));
                    dc.DrawPath(color, color, 2d, String.Format("M {0} m 2,6 l -2,4 l 7,1 l -4,-6 l -1,2 ", startPos.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if (Type == TransistorType.PNP)
                {
                    startPos = Point.Add(StartLocation, new Vector(-10d, (EndLocation.Y - StartLocation.Y) / 2));
                    if (startPos.Y % 10 != 0)
                        startPos.Y = startPos.Y + 5d;
                    dc.DrawLine(color, 2d, StartLocation, EndLocation);
                    dc.DrawEllipse(Colors.White, color, 2d, startPos, 16d, 16d);
                    dc.DrawLine(color, 3d, new Point(startPos.X - 6d, startPos.Y - 10d), new Point(startPos.X - 6d, startPos.Y + 10d));
                    dc.DrawLine(color, 2d, new Point(startPos.X - 30d, startPos.Y), new Point(startPos.X - 6d, startPos.Y));
                    dc.DrawLine(color, 2d, new Point(StartLocation.X + 1d, startPos.Y - 13d), new Point(startPos.X - 6d, startPos.Y - 4d));
                    dc.DrawLine(color, 2d, new Point(StartLocation.X + 1d, startPos.Y + 13d), new Point(startPos.X - 6d, startPos.Y + 4d));
                    dc.DrawPath(color, color, 2d, String.Format("M {0} m 2,-10 l 2,4 l -7,1 l 4,-6 l 1,2 ", startPos.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if (Type == TransistorType.NChannel)
                {
                    dc.DrawLine(color, 2f, StartLocation, startPos);
                    dc.DrawPath(null, color, 4.0f, String.Format("M {0} m -18,12 l 0,8 m 0,4 l 0,8 m 0,4 l 0,8", startPos.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    dc.DrawPath(null, color, 2.0f, String.Format("M {0} l 0,16 l -16,0 m -8,0 l 0,24 l -16,0 m 24,0 l 16,0 l 0,8 l 12,0 l 0,-20 l -28,0 m 16,20 L {1}", startPos.ToString(System.Globalization.CultureInfo.InvariantCulture), EndLocation.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    dc.DrawPath(color, color, 2.0f, String.Format("M {0} m -15,28 l 4,3 l 0,-6 l -4,3 l 2,2", startPos.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    dc.DrawEllipse(color, color, 1f, Point.Add(startPos, new Vector(0d, 48d)), 3f, 3f);
                }
                else if (Type == TransistorType.PChannel)
                {
                    dc.DrawLine(color, 2f, StartLocation, startPos);
                    dc.DrawPath(null, color, 4.0f, String.Format("M {0} m -18,12 l 0,8 m 0,4 l 0,8 m 0,4 l 0,8", startPos.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    dc.DrawPath(null, color, 2.0f, String.Format("M {0} l 0,16 l -16,0 m -8,0 l 0,24 l -16,0 m 24,0 l 16,0 l 0,8 L {1}", startPos.ToString(System.Globalization.CultureInfo.InvariantCulture), EndLocation.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    dc.DrawPath(null, color, 2.0f, String.Format("M {0} m -20,28 l 32,0 l 0,-20 l -12,0", startPos.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    dc.DrawPath(color, color, 2.0f, String.Format("M {0} m -5,28 l -4,3 l 0,-6 l 4,3 l -2,2", startPos.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    dc.DrawEllipse(color, color, 1f, Point.Add(startPos, new Vector(0d, 8d)), 3f, 3f);
                }
            }
            else
            {
                dc.DrawRectangle(Colors.Transparent, color, 1d, BoundingBox);
                dc.DrawText("This component can only be displayed vertically.", "Arial", 10d, color, Point.Add(StartLocation, new Vector(2d, -5d)));
            }
        }

        public enum TransistorType
        {
            NPN,
            PNP,
            NChannel,
            PChannel
        }
    }
}
