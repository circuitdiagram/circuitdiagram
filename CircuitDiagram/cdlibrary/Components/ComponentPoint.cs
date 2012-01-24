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
using System.Windows;
using System.Text.RegularExpressions;

namespace CircuitDiagram.Components
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

        public ComponentPoint(string x, string y)
        {
            RelativeToX = ComponentPosition.Absolute;
            RelativeToY = ComponentPosition.Absolute;
            Offset = new Vector();

            if (x.StartsWith("_Start", StringComparison.InvariantCultureIgnoreCase))
                RelativeToX = ComponentPosition.Start;
            if (y.StartsWith("_Start", StringComparison.InvariantCultureIgnoreCase))
                RelativeToY = ComponentPosition.Start;
            if (x.StartsWith("_Middle", StringComparison.InvariantCultureIgnoreCase))
                RelativeToX = ComponentPosition.Middle;
            if (y.StartsWith("_Middle", StringComparison.InvariantCultureIgnoreCase))
                RelativeToY = ComponentPosition.Middle;
            if (x.StartsWith("_End", StringComparison.InvariantCultureIgnoreCase))
                RelativeToX = ComponentPosition.End;
            if (y.StartsWith("_End", StringComparison.InvariantCultureIgnoreCase))
                RelativeToY = ComponentPosition.End;

            Regex regex = new Regex("[\\-\\+][0-9\\.]+");
            Match xmatch = regex.Match(x);
            Match ymatch = regex.Match(y);

            if (xmatch.Captures.Count >= 1)
                Offset = new Vector(double.Parse(xmatch.Captures[0].Value), Offset.Y);
            if (ymatch.Captures.Count >= 1)
                Offset = new Vector(Offset.X, double.Parse(ymatch.Captures[0].Value));
        }

        public ComponentPoint(string point)
        {
            RelativeToX = ComponentPosition.Absolute;
            RelativeToY = ComponentPosition.Absolute;
            Offset = new Vector();

            Regex regex0 = new Regex("(_Start|_Middle|_End)[0-9-\\+xy\\.]+(_Start|_Middle|_End)");
            Regex regex1 = new Regex("(_Start|_Middle|_End)[0-9-\\+xy\\.]+");
            Regex regex2 = new Regex("(_Start|_Middle|_End)");
            if (regex0.IsMatch(point))
            {
                // Not supported
            }
            else if (regex1.IsMatch(point))
            {
                if (point.StartsWith("_Middle", StringComparison.InvariantCultureIgnoreCase))
                {
                    RelativeToX = ComponentPosition.Middle;
                    RelativeToY = ComponentPosition.Middle;
                }
                else if (point.StartsWith("_End", StringComparison.InvariantCultureIgnoreCase))
                {
                    RelativeToX = ComponentPosition.End;
                    RelativeToY = ComponentPosition.End;
                }

                Regex xoffset = new Regex("[\\+\\-0-9\\.]+x");
                Regex yoffset = new Regex("[\\+\\-0-9\\.]+y");

                Match xmatch = xoffset.Match(point);
                Match ymatch = yoffset.Match(point);

                if (xmatch.Captures.Count >= 1)
                    Offset = new Vector(double.Parse(xmatch.Captures[0].Value.Replace("x", "")), Offset.Y);
                if (ymatch.Captures.Count >= 1)
                    Offset = new Vector(Offset.X, double.Parse(ymatch.Captures[0].Value.Replace("y", "")));
            }
            else if (regex2.IsMatch(point))
            {
                if (point.StartsWith("_Start", StringComparison.InvariantCultureIgnoreCase))
                {
                    RelativeToX = ComponentPosition.Start;
                    RelativeToY = ComponentPosition.Start;
                }
                else if (point.StartsWith("_Middle", StringComparison.InvariantCultureIgnoreCase))
                {
                    RelativeToX = ComponentPosition.Middle;
                    RelativeToY = ComponentPosition.Middle;
                }
                else if (point.StartsWith("_End", StringComparison.InvariantCultureIgnoreCase))
                {
                    RelativeToX = ComponentPosition.End;
                    RelativeToY = ComponentPosition.End;
                }
            }
        }

        public Point Resolve(Component component)
        {
            double x = Offset.X;
            double y = Offset.Y;

            if (RelativeToX != ComponentPosition.Absolute)
                x += component.StartLocation.X;
            if (RelativeToY != ComponentPosition.Absolute)
                y += component.StartLocation.Y;

            if (RelativeToX == ComponentPosition.Middle && component.Horizontal)
                x += component.Size / 2;
            else if (RelativeToY == ComponentPosition.Middle && !component.Horizontal)
                y += component.Size / 2;

            if (RelativeToX == ComponentPosition.End && component.Horizontal)
                x += component.Size;
            else if (RelativeToY == ComponentPosition.End && !component.Horizontal)
                y += component.Size;

            FlagOptions flags = ComponentHelper.ApplyFlags(component);
            if ((flags & FlagOptions.MiddleMustAlign) == FlagOptions.MiddleMustAlign)
            {
                if (component.Horizontal && this.RelativeToX == ComponentPosition.Middle)
                {
                    if ((x - Offset.X) % ComponentHelper.GridSize != 0d)
                        x += 5d;
                }
                else if (this.RelativeToY == ComponentPosition.Middle)
                {
                    if ((y - Offset.Y) % ComponentHelper.GridSize != 0d)
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
    }

    public enum ComponentPosition
    {
        Absolute = 0,
        Start = 1,
        Middle = 2,
        End = 3
    }
}
