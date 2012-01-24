using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using CircuitDiagram.IO;

namespace CircuitDiagram.Components.Render.Path
{
    class SmoothQuadraticBeizerCurveTo : IPathCommand
    {
        public Point ControlStart { get; set; }
        public Point ControlEnd { get; set; }
        public Point End { get; set; }

        public CommandType Type
        {
            get { return CommandType.SmoothQuadraticBeizerCurveTo; }
        }

        public void Draw(StreamGeometryContext dc, Vector startOffset)
        {
            dc.BezierTo(Point.Add(ControlStart, startOffset), Point.Add(ControlEnd, startOffset), Point.Add(End, startOffset), true, true);
        }

        public string Shorthand(Point offset, Point previous)
        {
            return String.Format("c {0},{1} {2},{3} {4},{5}", ControlStart.X, ControlStart.Y, ControlEnd.X, ControlEnd.Y, End.X, End.Y);
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
