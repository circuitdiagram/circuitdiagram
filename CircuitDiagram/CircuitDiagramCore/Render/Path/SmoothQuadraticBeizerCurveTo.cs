using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CircuitDiagram.Render.Path
{
    public class SmoothQuadraticBeizerCurveTo : IPathCommand
    {
        public Point ControlStart { get; set; }
        public Point ControlEnd { get; set; }
        public Point End { get; set; }

        public CommandType Type
        {
            get { return CommandType.SmoothQuadraticBeizerCurveTo; }
        }

        public SmoothQuadraticBeizerCurveTo()
        {
            ControlStart = new Point();
            ControlEnd = new Point();
            End = new Point();
        }

        public SmoothQuadraticBeizerCurveTo(double x1, double y1, double x2, double y2, double x, double y)
        {
            ControlStart = new Point(x1, y1);
            ControlEnd = new Point(x2, y2);
            End = new Point(x, y);
        }

        public string Shorthand(Point offset, Point previous)
        {
            return String.Format("c {0},{1} {2},{3} {4},{5}", ControlStart.X, ControlStart.Y, ControlEnd.X, ControlEnd.Y, End.X, End.Y);
        }

        public IPathCommand Flip(bool horizontal)
        {
            if (horizontal)
            {
                return new SmoothQuadraticBeizerCurveTo(-ControlStart.X, ControlStart.Y, -ControlEnd.X, ControlEnd.Y, -End.X, End.Y);
            }
            else
            {
                return new SmoothQuadraticBeizerCurveTo(ControlStart.X, -ControlStart.Y, ControlEnd.X, -ControlEnd.Y, End.X, -End.Y);
            }
        }
    }
}
