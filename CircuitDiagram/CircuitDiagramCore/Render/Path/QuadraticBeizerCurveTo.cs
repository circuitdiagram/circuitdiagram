using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CircuitDiagram.Render.Path
{
    public class QuadraticBeizerCurveTo : IPathCommand
    {
        public Point Control { get; set; }
        public Point End { get; set; }

        public CommandType Type
        {
            get { return CommandType.QuadraticBeizerCurveTo; }
        }

        public QuadraticBeizerCurveTo()
        {
            Control = new Point();
            End = new Point();
        }

        public QuadraticBeizerCurveTo(double x1, double y1, double x, double y)
        {
            Control = new Point(x1, y1);
            End = new Point(x, y);
        }

        public string Shorthand(Point offset, Point previous)
        {
            return String.Format("Q {0},{1} {2},{3}", Control.X + offset.X, Control.Y + offset.Y, End.X + offset.X, End.Y + offset.Y);
        }

        public IPathCommand Flip(bool horizontal)
        {
            if (horizontal)
            {
                return new QuadraticBeizerCurveTo(-Control.X, Control.Y, -End.X, End.Y);
            }
            else
            {
                return new QuadraticBeizerCurveTo(Control.X, -Control.Y, End.X, -End.Y);
            }
        }
    }
}
