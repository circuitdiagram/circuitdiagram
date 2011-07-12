// Range.cs
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

namespace CircuitDiagram
{
    public class Range
    {
        internal Point fixedStart = new Point(0, 0);
        private Point fixedEnd = new Point(0, 0);
        internal Point start = new Point(0, 0);
        internal Point end = new Point(0, 0);

        public Point Anchor
        {
            get { return fixedStart; }
            set { fixedStart = value; }
        }

        public Point Start
        {
            get { return start; }
        }

        public Point End
        {
            get { return end; }
            set
            {
                fixedEnd = value;

                if (value.X >= fixedStart.X && value.Y >= fixedStart.Y)
                {
                    end = value;
                    start = fixedStart;
                }
                else if (value.X >= fixedStart.X && value.Y < fixedStart.Y)
                {
                    end.X = value.X;
                    end.Y = fixedStart.Y;
                    start.Y = value.Y;
                }
                else if (value.X < fixedStart.X && value.Y >= fixedStart.Y)
                {
                    end.Y = value.Y;
                    end.X = fixedStart.X;
                    start.X = value.X;
                }
                else
                {
                    Point preStart = fixedStart;
                    end = preStart;
                    start = value;
                }
            }
        }

        public Size Size
        {
            get { return new Size(End.X - Start.X, End.Y - Start.Y); }
        }

        //public Size Offset
        //{
        //    get { return new Size(fixedEnd.X - Anchor.X, fixedEnd.Y - Anchor.Y); }
        //}

        public Range(Point start, Point end)
        {
            fixedStart = start;
            this.start = start;
            End = end;
        }

        public Range(int startX, int startY, int endX, int endY)
        {
            fixedStart = new Point(startX, startY);
            this.start = new Point(startX, startY);
            End = new Point(endX, endY);
        }

        public Range(Point startFinish)
        {
            fixedStart = startFinish;
            this.start = startFinish;
            End = startFinish;
        }

        public Range(int x, int y)
        {
            fixedStart = new Point(x, y);
            this.start = new Point(x, y);
            End = new Point(x, y);
        }

        public static Range Zero
        {
            get { return new Range(0, 0); }
        }

        public override string ToString()
        {
            return "Start: " + start.X.ToString() + ", " + start.Y.ToString() + ", End: " + end.X.ToString() + ", " + end.Y.ToString();
        }
    }
}
