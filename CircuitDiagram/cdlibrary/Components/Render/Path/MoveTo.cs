using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using CircuitDiagram.IO;

namespace CircuitDiagram.Components.Render.Path
{
    class MoveTo : IPathCommand
    {
        public CommandType Type { get { return CommandType.MoveTo; } }
        public Point End { get { return new Point(X, Y); } }

        public double X { get; set; }
        public double Y { get; set; }

        public MoveTo()
        {
            X = 0;
            Y = 0;
        }

        public MoveTo(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void Draw(StreamGeometryContext dc, Vector startOffset)
        {
            dc.LineTo(new System.Windows.Point(X + startOffset.X, Y + startOffset.Y), false, true);
        }

        public string Shorthand(Point offset, Point previous)
        {
            double x = X + offset.X;
            double y = Y + offset.Y;

            return "M " + x.ToString() + "," + y.ToString();
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
        }

        public void Read(System.IO.BinaryReader reader)
        {
            X = reader.ReadDouble();
            Y = reader.ReadDouble();
        }
    }
}
