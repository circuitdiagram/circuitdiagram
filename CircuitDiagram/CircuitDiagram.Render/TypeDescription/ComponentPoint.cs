// ComponentPoint.cs
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
using System.Text.RegularExpressions;
using CircuitDiagram.Circuit;
using CircuitDiagram.Drawing;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.TypeDescription
{
    public class ComponentPoint
    {
        public ComponentPosition RelativeToX { get; set; }
        public ComponentPosition RelativeToY { get; set; }
        public Vector Offset { get; set; }

        public ComponentPoint()
        {
            RelativeToX = ComponentPosition.Start;
            RelativeToY = ComponentPosition.Start;
            Offset = new Vector();
        }

        public ComponentPoint(ComponentPosition relativeToX, ComponentPosition relativeToY, Vector offset)
        {
            RelativeToX = relativeToX;
            RelativeToY = relativeToY;
            Offset = offset;
        }
        
        public Point Resolve(LayoutInformation layout, LayoutOptions options)
        {
            ComponentPoint tempPoint = this;
            if (layout.IsFlipped)
                tempPoint = Flip(layout.Orientation == Orientation.Horizontal);

            double x = tempPoint.Offset.X;
            double y = tempPoint.Offset.Y;

            if (tempPoint.RelativeToX == ComponentPosition.Middle && layout.Orientation == Orientation.Horizontal)
                x += layout.Size / 2;
            else if (tempPoint.RelativeToY == ComponentPosition.Middle && layout.Orientation == Orientation.Vertical)
                y += layout.Size / 2;

            if (tempPoint.RelativeToX == ComponentPosition.End && layout.Orientation == Orientation.Horizontal)
                x += layout.Size;
            else if (tempPoint.RelativeToY == ComponentPosition.End && layout.Orientation == Orientation.Vertical)
                y += layout.Size;
            
            if (options.AlignMiddle)
            {
                if (layout.Orientation == Orientation.Horizontal && tempPoint.RelativeToX == ComponentPosition.Middle)
                {
                    if ((x - tempPoint.Offset.X) % options.GridSize != 0d)
                        x += 5d;
                }
                else if (tempPoint.RelativeToY == ComponentPosition.Middle)
                {
                    if ((y - tempPoint.Offset.Y) % options.GridSize != 0d)
                        y += 5d;
                }
            }

            return new Point(x, y);
        }

        public override string ToString()
        {
            string xoffset = Offset.X.ToString();
            if (Offset.X >= 0)
                xoffset = "+" + Offset.X.ToString();
            string yoffset = Offset.Y.ToString();
            if (Offset.Y >= 0)
                yoffset = "+" + Offset.Y.ToString();

            return String.Format("{0}{1},{2}{3}", RelativeToX, xoffset, RelativeToY, yoffset);
        }

        public override bool Equals(object obj)
        {
            ComponentPoint other = obj as ComponentPoint;
            if (other != null)
                return (other.Offset == this.Offset && other.RelativeToX == this.RelativeToX && other.RelativeToY == this.RelativeToY);
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public ComponentPoint Flip(bool horizontal)
        {
            ComponentPoint returnPoint = new ComponentPoint();
            if (horizontal)
                returnPoint.Offset = new Vector(-Offset.X, Offset.Y);
            else
                returnPoint.Offset = new Vector(Offset.X, -Offset.Y);

            if (RelativeToX == ComponentPosition.Start)
                returnPoint.RelativeToX = ComponentPosition.End;
            else if (RelativeToX == ComponentPosition.Middle)
                returnPoint.RelativeToX = ComponentPosition.Middle;
            else if (RelativeToX == ComponentPosition.End)
                returnPoint.RelativeToX = ComponentPosition.Start;

            if (RelativeToY == ComponentPosition.Start)
                returnPoint.RelativeToY = ComponentPosition.End;
            else if (RelativeToY == ComponentPosition.Middle)
                returnPoint.RelativeToY = ComponentPosition.Middle;
            else if (RelativeToY == ComponentPosition.End)
                returnPoint.RelativeToY = ComponentPosition.Start;

            return returnPoint;
        }
    }

    public enum ComponentPosition
    {
        Absolute = 0,
        Start = 1,
        Middle = 2,
        End = 3
    }
}
