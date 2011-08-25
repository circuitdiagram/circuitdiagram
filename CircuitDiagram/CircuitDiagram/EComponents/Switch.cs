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
                if (Type == SwitchType.Analogue)
                {
                    if (Horizontal)
                        return new Rect(new Point(StartLocation.X, StartLocation.Y - 40), new Size(EndLocation.X - StartLocation.X, 55));
                    else
                        return new Rect(new Point(StartLocation.X - 15, StartLocation.Y), new Size(55, EndLocation.Y - StartLocation.Y));
                }
                else if (Type == SwitchType.PushToBreak)
                {
                    if (Horizontal)
                        return new Rect(new Point(StartLocation.X, StartLocation.Y - 5d), new Point(EndLocation.X, EndLocation.Y + 5d));
                    else
                        return new Rect(new Point(StartLocation.X - 5d, StartLocation.Y), new Point(EndLocation.X + 5d, EndLocation.Y));
                }
                else if (Type == SwitchType.Changeover)
                {
                    if (Horizontal)
                        return new Rect(new Point(StartLocation.X, StartLocation.Y - 10d), new Point(EndLocation.X, EndLocation.Y + 10d));
                    else
                        return new Rect(new Point(StartLocation.X - 10d, StartLocation.Y), new Point(EndLocation.X + 10d, EndLocation.Y));
                }
                else
                {
                    if (Horizontal)
                        return new Rect(new Point(StartLocation.X, StartLocation.Y - 15), new Size(EndLocation.X - StartLocation.X, 18));
                    else
                        return new Rect(new Point(StartLocation.X - 15, StartLocation.Y), new Size(18, EndLocation.Y - StartLocation.Y));
                }
            }
        }

        [ComponentSerializable("t")]
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

        public override void UpdateLayout()
        {
            this.ImplementMinimumSize(30d);
        }

        public override void Render(IRenderer dc, Color colour)
        {
            if (Horizontal)
            {
                Point gapStart = new Point(StartLocation.X + Size.Width / 2 - 12, StartLocation.Y);
                Point gapEnd = new Point(StartLocation.X + Size.Width / 2 + 12, StartLocation.Y);
                if (Type == SwitchType.Analogue)
                {
                    gapStart = new Point(StartLocation.X + Size.Width / 2 - 8, StartLocation.Y);
                    gapEnd = new Point(StartLocation.X + Size.Width / 2 + 8, StartLocation.Y);
                    if ((gapStart.X + 8d) % 10 != 0)
                    {
                        gapStart.X += 5d;
                        gapEnd.X += 5d;
                    }
                }
                dc.DrawLine(colour, 2d, StartLocation, gapStart);
                dc.DrawEllipse(Colors.White, colour, 2d, gapStart, 3d, 3d);
                if (Type != SwitchType.Changeover)
                {
                    dc.DrawLine(colour, 2d, gapEnd, EndLocation);
                    dc.DrawEllipse(Colors.White, colour, 2d, gapEnd, 3d, 3d);
                }
                if (Type == SwitchType.Push)
                {
                    dc.DrawPath(null, colour, 2f, "M " + gapStart.ToString() + " m -2,-8 l 28,0 m -14,0 l 0,-6 m -6,0 l 12,0");
                }
                else if (Type == SwitchType.Toggle)
                {
                    dc.DrawLine(colour, 2d, Point.Add(gapStart, new Vector(3d, -1d)), Point.Add(gapEnd, new Vector(0, -8d)));
                }
                else if (Type == SwitchType.Analogue)
                {
                    dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(gapStart.X - 6d, gapStart.Y - 14d, 28d, 28d));
                    dc.DrawLine(colour, 3d, new Point(gapStart.X - 2d, gapStart.Y + 10d), new Point(gapEnd.X + 2d, gapEnd.Y + 10d));
                    dc.DrawLine(colour, 2d, new Point(gapStart.X + 8d, StartLocation.Y - 40d), new Point(gapStart.X + 8d, StartLocation.Y - 14d));
                }
                else if (Type == SwitchType.PushToBreak)
                {
                    dc.DrawPath(null, colour, 2f, "M " + gapStart.ToString() + " m -2,5 l 28,0 m -14,0 l 0,-6 m -6,0 l 12,0");
                }
                else if (Type == SwitchType.Changeover)
                {
                    dc.DrawLine(colour, 2d, Point.Add(gapStart, new Vector(3d, -1d)), Point.Add(gapEnd, new Vector(0, -10d)));
                    dc.DrawLine(colour, 2d, new Point(gapEnd.X, gapEnd.Y - 10d), new Point(EndLocation.X, EndLocation.Y - 10d));
                    dc.DrawEllipse(Colors.White, colour, 2d, new Point(gapEnd.X, gapEnd.Y - 10d), 3d, 3d);
                    dc.DrawLine(colour, 2d, new Point(gapEnd.X, gapEnd.Y + 10d), new Point(EndLocation.X, EndLocation.Y + 10d));
                    dc.DrawEllipse(Colors.White, colour, 2d, new Point(gapEnd.X, gapEnd.Y + 10d), 3d, 3d);
                }
            }
            if (!Horizontal)
            {
                Point gapStart = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 12);
                Point gapEnd = new Point(StartLocation.X, StartLocation.Y+ Size.Height / 2 + 12);
                if (Type == SwitchType.Analogue)
                {
                    gapStart = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 - 8);
                    gapEnd = new Point(StartLocation.X, StartLocation.Y + Size.Height / 2 + 8);
                    if ((gapStart.Y + 8d) % 10 != 0)
                    {
                        gapStart.Y += 5d;
                        gapEnd.Y += 5d;
                    }
                }
                dc.DrawLine(colour, 2d, StartLocation, gapStart);
                dc.DrawEllipse(Colors.White, colour, 2d, gapStart, 3d, 3d);
                if (Type != SwitchType.Changeover)
                {
                    dc.DrawLine(colour, 2d, gapEnd, EndLocation);
                    dc.DrawEllipse(Colors.White, colour, 2d, gapEnd, 3d, 3d);
                }
                if (Type == SwitchType.Push)
                {
                    dc.DrawPath(null, colour, 2f, "M " + gapStart.ToString() + " m -8,-2 l 0,28 m 0,-14 l -6,0 m 0,-6 l 0,12");
                }
                else if (Type == SwitchType.Toggle)
                {
                    dc.DrawLine(colour, 2d, Point.Add(gapStart, new Vector(-1d, 3d)), Point.Add(gapEnd, new Vector(-8d, 0)));
                }
                else if (Type == SwitchType.Analogue)
                {
                    dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(gapStart.X - 14d, gapStart.Y - 6d, 28d, 28d));
                    dc.DrawLine(colour, 3d, new Point(gapStart.X - 10d, gapStart.Y - 2d), new Point(gapEnd.X - 10d, gapEnd.Y + 2d));
                    dc.DrawLine(colour, 2d, new Point(gapStart.X + 14d, gapStart.Y + 8d), new Point(gapStart.X + 40d, gapStart.Y + 8d));
                }
                else if (Type == SwitchType.PushToBreak)
                {
                    dc.DrawPath(null, colour, 2f, "M " + gapStart.ToString() + " m 5,-2 l 0,28 m 0,-14 l -6,0 m 0,-6 l 0,12");
                }
                else if (Type == SwitchType.Changeover)
                {
                    dc.DrawLine(colour, 2d, Point.Add(gapStart, new Vector(0d, 3d)), Point.Add(gapEnd, new Vector(-10d, 0)));
                    dc.DrawLine(colour, 2d, new Point(gapEnd.X - 10d, gapEnd.Y), new Point(EndLocation.X - 10d, EndLocation.Y));
                    dc.DrawEllipse(Colors.White, colour, 2d, new Point(gapEnd.X - 10d, gapEnd.Y), 3d, 3d);
                    dc.DrawLine(colour, 2d, new Point(gapEnd.X + 10d, gapEnd.Y), new Point(EndLocation.X + 10d, EndLocation.Y));
                    dc.DrawEllipse(Colors.White, colour, 2d, new Point(gapEnd.X + 10d, gapEnd.Y), 3d, 3d);
                }
            }
        }
    }

    public enum SwitchType
    {
        Push = 0,
        Toggle = 1,
        Analogue = 2,
        PushToBreak = 3,
        Changeover = 4
    }
}
