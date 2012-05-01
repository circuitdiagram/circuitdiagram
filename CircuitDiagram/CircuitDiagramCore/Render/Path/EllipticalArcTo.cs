using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CircuitDiagram.Render.Path
{
    public class EllipticalArcTo : IPathCommand
    {
        public Size Size {get; set;}
        public Point End { get; set; }
        public double RotationAngle {get; set;}
        public bool IsLargeArc {get; set;}
        public SweepDirection SweepDirection {get; set;}

        public CommandType Type
        {
            get { return CommandType.EllipticalArcTo; }
        }

        public EllipticalArcTo()
        {
            Size = new Size();
            RotationAngle = 0;
            IsLargeArc = false;
            SweepDirection = SweepDirection.Clockwise;
            End = new Point();
        }

        public EllipticalArcTo(double rx,double ry, double xRotation, bool isLargeArc, bool sweep, double x, double y)
        {
            Size = new Size(rx, ry);
            RotationAngle = xRotation;
            IsLargeArc = isLargeArc;
            SweepDirection = (sweep ? SweepDirection.Clockwise : SweepDirection.Counterclockwise);
            End = new Point(x, y);
        }

        public string Shorthand(Point offset, Point previous)
        {
            return String.Format("A {0},{1} {2} {3} {4} {5},{6}", Size.Width, Size.Height, RotationAngle, (IsLargeArc ? "1" : "0"), (SweepDirection == SweepDirection.Clockwise ? "1" : "0"), End.X + offset.X, End.Y + offset.Y);
        }

        public IPathCommand Flip(bool horizontal)
        {
            if (horizontal)
            {
                return new EllipticalArcTo(Size.Width, Size.Height, RotationAngle, IsLargeArc, SweepDirection != SweepDirection.Clockwise, -End.X, End.Y);
            }
            else
            {
                return new EllipticalArcTo(Size.Width, Size.Height, RotationAngle, IsLargeArc, SweepDirection != SweepDirection.Clockwise, End.X, -End.Y);
            }
        }
    }

    public enum SweepDirection
    {
        Clockwise,
        Counterclockwise
    }
}
