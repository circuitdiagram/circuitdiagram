using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.IO;

namespace CircuitDiagram.Render.Path
{
    public class ClosePath : IPathCommand
    {
        public Point End { get { return new Point(); } }

        public CommandType Type
        {
            get { return CommandType.ClosePath; }
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

        public IPathCommand Flip(bool horizontal)
        {
            throw new NotImplementedException();
        }
    }
}
