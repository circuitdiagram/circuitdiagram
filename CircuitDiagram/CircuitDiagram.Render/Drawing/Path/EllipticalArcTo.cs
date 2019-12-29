// EllipticalArcTo.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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
using CircuitDiagram.Primitives;

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

        public IPathCommand Reflect()
        {
            return new EllipticalArcTo(Size.Height, Size.Width, RotationAngle, IsLargeArc, SweepDirection != SweepDirection.Clockwise, End.Y, End.X);
        }
    }

    public enum SweepDirection
    {
        Clockwise,
        Counterclockwise
    }
}
