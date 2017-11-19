// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using System.Windows.Media;
using CircuitDiagram.Render.Path;
using Point = System.Windows.Point;

namespace CircuitDiagram.Render
{
    static class RenderHelper
    {
        public static Geometry GetGeometry(Point start, IList<IPathCommand> commands, bool fill)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (var dc = geometry.Open())
            {
                dc.BeginFigure(start, fill, false);
                Vector startOffset = new Vector(start.X, start.Y);
                IPathCommand previous = null;
                foreach (IPathCommand command in commands)
                {
                    Draw(dc, command, previous, startOffset);
                    previous = command;
                }
                dc.Close();
            }
            return geometry;
        }

        private static void Draw(StreamGeometryContext dc, IPathCommand element, IPathCommand previous, Vector startOffset)
        {
            if (element is LineTo)
                dc.LineTo(new System.Windows.Point((element as LineTo).X + startOffset.X, (element as LineTo).Y + startOffset.Y), true, true);
            else if (element is MoveTo)
                dc.LineTo(new System.Windows.Point((element as MoveTo).X + startOffset.X, (element as MoveTo).Y + startOffset.Y), false, true);
            else if (element is CurveTo)
            {
                Point controlStart = Point.Add((element as CurveTo).ControlStart.ToWinPoint(), startOffset);
                Point controlEnd = Point.Add((element as CurveTo).ControlEnd.ToWinPoint(), startOffset);
                Point end = Point.Add((element as CurveTo).End.ToWinPoint(), startOffset);
                dc.BezierTo(controlStart, controlEnd, end, true, true);
            }
            else if (element is EllipticalArcTo)
            {
                dc.ArcTo(Point.Add((element as EllipticalArcTo).End.ToWinPoint(), startOffset), (element as EllipticalArcTo).Size.ToWinSize(), (element as EllipticalArcTo).RotationAngle, (element as EllipticalArcTo).IsLargeArc, (element as EllipticalArcTo).SweepDirection == Path.SweepDirection.Clockwise ? System.Windows.Media.SweepDirection.Clockwise : System.Windows.Media.SweepDirection.Counterclockwise, true, true);
            }
            else if (element is SmoothCurveTo)
            {
                SmoothCurveTo item = element as SmoothCurveTo;

                // If previous command is not S or C, then control points are the same
                Point controlStart = item.ControlEnd.ToWinPoint();

                // Else reflect ControlEnd of previous command in StartPoint
                if (previous is SmoothCurveTo)
                    controlStart = ReflectPointIn((previous as SmoothCurveTo).ControlEnd.ToWinPoint(), (previous as SmoothCurveTo).End.ToWinPoint());
                else if (previous is CurveTo)
                    controlStart = ReflectPointIn((previous as CurveTo).ControlEnd.ToWinPoint(), (previous as CurveTo).End.ToWinPoint());

                dc.BezierTo(Point.Add(controlStart, startOffset), Point.Add(item.ControlEnd.ToWinPoint(), startOffset), Point.Add(item.End.ToWinPoint(), startOffset), true, true);
            }
            else if (element is SmoothQuadraticBeizerCurveTo)
            {
                SmoothQuadraticBeizerCurveTo item = element as SmoothQuadraticBeizerCurveTo;

                if (previous is SmoothQuadraticBeizerCurveTo)
                {
                    throw new NotSupportedException();
                }
                else if (previous is QuadraticBeizerCurveTo)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    // If previous command is not Q or T, then draw a line
                    dc.LineTo(item.End.ToWinPoint(), true, true);
                }
            }
            else if (element is QuadraticBeizerCurveTo)
            {
                QuadraticBeizerCurveTo item = element as QuadraticBeizerCurveTo;
                dc.QuadraticBezierTo(Point.Add(item.Control.ToWinPoint(), startOffset), Point.Add(item.End.ToWinPoint(), startOffset), true, true);
            }
        }

        public static string CircuitFont()
        {
            return "Arial";
        }

        private static Point ReflectPointIn(Point point, Point anchor)
        {
            Vector magnitude = Point.Subtract(point, anchor);

            // Reflect
            magnitude.Negate();

            return Point.Add(anchor, magnitude);
        }
    }
}
