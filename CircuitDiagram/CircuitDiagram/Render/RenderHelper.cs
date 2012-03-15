using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using CircuitDiagram.Components.Render.Path;

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
                foreach (IPathCommand command in commands)
                {
                    command.Draw(dc, startOffset);
                }
                dc.Close();
            }
            return geometry;
        }

        public static void Draw(this IPathCommand element, StreamGeometryContext dc, Vector startOffset)
        {
            if (element is LineTo)
                dc.LineTo(new System.Windows.Point((element as LineTo).X + startOffset.X, (element as LineTo).Y + startOffset.Y), true, true);
            else if (element is MoveTo)
                dc.LineTo(new System.Windows.Point((element as MoveTo).X + startOffset.X, (element as MoveTo).Y + startOffset.Y), false, true);
            else if (element is CurveTo)
            {
                Point controlStart = Point.Add((element as CurveTo).ControlStart, startOffset);
                Point controlEnd = Point.Add((element as CurveTo).ControlEnd, startOffset);
                Point end = Point.Add((element as CurveTo).End, startOffset);
                dc.BezierTo(controlStart, controlEnd, end, true, true);
            }
            else if (element is EllipticalArcTo)
            {
                dc.ArcTo(Point.Add((element as EllipticalArcTo).End, startOffset), (element as EllipticalArcTo).Size, (element as EllipticalArcTo).RotationAngle, (element as EllipticalArcTo).IsLargeArc, (element as EllipticalArcTo).SweepDirection == Components.Render.Path.SweepDirection.Clockwise ? System.Windows.Media.SweepDirection.Clockwise : System.Windows.Media.SweepDirection.Counterclockwise, true, true);
            }
            else if (element is SmoothCurveTo)
            {
                SmoothCurveTo item = element as SmoothCurveTo;
                dc.BezierTo(Point.Add(item.ControlStart, startOffset), Point.Add(item.ControlEnd, startOffset), Point.Add(item.End, startOffset), true, true);
            }
            else if (element is SmoothQuadraticBeizerCurveTo)
            {
                SmoothQuadraticBeizerCurveTo item = element as SmoothQuadraticBeizerCurveTo;
                dc.BezierTo(Point.Add(item.ControlStart, startOffset), Point.Add(item.ControlEnd, startOffset), Point.Add(item.End, startOffset), true, true);
            }
            else if (element is QuadraticBeizerCurveTo)
            {
                QuadraticBeizerCurveTo item = element as QuadraticBeizerCurveTo;
                dc.QuadraticBezierTo(Point.Add(item.Control, startOffset), Point.Add(item.End, startOffset), true, true);
            }
        }

        public static string CircuitFont()
        {
            return "Arial";
        }
    }
}
