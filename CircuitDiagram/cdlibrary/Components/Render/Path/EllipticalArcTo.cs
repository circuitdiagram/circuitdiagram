using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using CircuitDiagram.IO;

namespace CircuitDiagram.Components.Render.Path
{
    class EllipticalArcTo : IPathCommand
    {
        public Size Size {get; set;}
        public Point End { get; set; }
        double RotationAngle {get; set;}
        bool IsLargeArc {get; set;}
        SweepDirection SweepDirection {get; set;}

        public CommandType Type
        {
            get { return CommandType.EllipticalArcTo; }
        }

        public EllipticalArcTo()
        {
            Size = new Size();
            RotationAngle = 0;
            IsLargeArc = false;
            SweepDirection = System.Windows.Media.SweepDirection.Clockwise;
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

        public void Draw(StreamGeometryContext dc, Vector startOffset)
        {
            dc.ArcTo(Point.Add(End, startOffset), Size, RotationAngle, IsLargeArc, SweepDirection, true, true);
        }

        public string Shorthand(Point offset, Point previous)
        {
            return String.Format("a {0},{1} {2} {3} {4} {5},{6}", Size.Width, Size.Height, RotationAngle, (IsLargeArc ? "1" : "0"), (SweepDirection == System.Windows.Media.SweepDirection.Clockwise ? "1" : "0"), End.X, End.Y);
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(Size.Width);
            writer.Write(Size.Height);
            writer.Write(End);
            writer.Write(RotationAngle);
            writer.Write(IsLargeArc);
            writer.Write(SweepDirection == System.Windows.Media.SweepDirection.Clockwise);
        }

        public void Read(System.IO.BinaryReader reader)
        {
            Size = new Size(reader.ReadDouble(), reader.ReadDouble());
            End = reader.ReadPoint();
            RotationAngle = reader.ReadDouble();
            IsLargeArc = reader.ReadBoolean();
            SweepDirection = (reader.ReadBoolean() == false ? SweepDirection.Counterclockwise : System.Windows.Media.SweepDirection.Clockwise);
        }
    }
}
