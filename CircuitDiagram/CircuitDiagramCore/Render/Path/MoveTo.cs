using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace CircuitDiagram.Render.Path
{
    public class MoveTo : IPathCommand
    {
        public CommandType Type { get { return CommandType.MoveTo; } }
        public Point End { get { return new Point(X, Y); } }

        public double X { get; set; }
        public double Y { get; set; }

        public MoveTo()
        {
            X = 0;
            Y = 0;
        }

        public MoveTo(double x, double y)
        {
            X = x;
            Y = y;
        }

        public string Shorthand(Point offset, Point previous)
        {
            double x = X + offset.X;
            double y = Y + offset.Y;

            return "M " + x.ToString() + "," + y.ToString();
        }

        public IPathCommand Flip(bool horizontal)
        {
            if (horizontal)
            {
                return new MoveTo(-X, Y);
            }
            else
            {
                return new MoveTo(X, -Y);
            }
        }
    }
}
