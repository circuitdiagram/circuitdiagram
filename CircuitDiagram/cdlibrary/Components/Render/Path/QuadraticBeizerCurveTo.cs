using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using CircuitDiagram.IO;

namespace CircuitDiagram.Components.Render.Path
{
    class QuadraticBeizerCurveTo : IPathCommand
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

        public void Draw(StreamGeometryContext dc, Vector startOffset)
        {
            dc.QuadraticBezierTo(Point.Add(Control, startOffset), Point.Add(End, startOffset), true, true);
        }

        public string Shorthand(Point offset, Point previous)
        {
            return String.Format("q {0},{1} {2},{3}", Control.X, Control.Y, End.X, End.Y);
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(Control);
            writer.Write(End);
        }

        public void Read(System.IO.BinaryReader reader)
        {
            Control = reader.ReadPoint();
            End = reader.ReadPoint();
        }
    }
}
