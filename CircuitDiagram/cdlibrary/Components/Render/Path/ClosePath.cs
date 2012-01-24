using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.IO;

namespace CircuitDiagram.Components.Render.Path
{
    class ClosePath : IPathCommand
    {
        public Point End { get { return new Point(); } }

        public CommandType Type
        {
            get { return CommandType.ClosePath; }
        }

        public void Draw(StreamGeometryContext dc, Vector startOffset)
        {
            dc.BeginFigure(new Point(startOffset.X, startOffset.Y), false, false);
        }

        public string Shorthand(Point offset, Point previous)
        {
            return "Z";
        }

        public void Write(BinaryWriter writer)
        {
        }

        public void Read(BinaryReader reader)
        {
        }
    }
}
