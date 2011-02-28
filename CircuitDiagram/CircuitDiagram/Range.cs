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
