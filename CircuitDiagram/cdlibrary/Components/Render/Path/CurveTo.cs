using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using CircuitDiagram.IO;

namespace CircuitDiagram.Components.Render.Path
{
    class CurveTo : IPathCommand
    {
        public Point ControlStart { get; set; }
        public Point ControlEnd { get; set; }
        public Point End { get; set; }

        public CommandType Type
        {
            get { return CommandType.CurveTo; }
        }

        public CurveTo()
        {
            ControlStart = new Point();
            ControlEnd = new Point();
            End = new Point();
        }

        public CurveTo(double x1, double y1, double x2, double y2, double x, double y)
        {
            ControlStart = new Point(x1, y1);
            ControlEnd = new Point(x2, y2);
            End = new Point(x, y);
        }

        public void Draw(StreamGeometryContext dc, Vector startOffset)
        {
            Point controlStart = Point.Add(ControlStart, startOffset);
            Point controlEnd = Point.Add(ControlEnd, startOffset);
            Point end = Point.Add(End, startOffset);
            dc.BezierTo(controlStart, controlEnd, end, true, true);
        }

        public string Shorthand(Point offset, Point previous)
        {
            return String.Format("c {0},{1} {2},{3} {4},{5}", ControlStart.X - previous.X, ControlStart.Y - previous.Y, ControlEnd.X - previous.X, ControlEnd.Y - previous.Y, End.X - previous.X, End.Y - previous.Y);
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(ControlStart);
            writer.Write(ControlEnd);
            writer.Write(End);
        }

        public void Read(System.IO.BinaryReader reader)
        {
            ControlStart = reader.ReadPoint();
            ControlEnd = reader.ReadPoint();
            End = reader.ReadPoint();
        }
    }
}
