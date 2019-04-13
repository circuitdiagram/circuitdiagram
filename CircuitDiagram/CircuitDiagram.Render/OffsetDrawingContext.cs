// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
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
using System.Text;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render.Path;

namespace CircuitDiagram.Render
{
    public class OffsetDrawingContext : IDrawingContext
    {
        private readonly Point offset;

        public OffsetDrawingContext(IDrawingContext underlying, Point offset)
        {
            this.offset = offset;
            Underlying = underlying;
        }

        public IDrawingContext Underlying { get; }

        public void DrawLine(Point start, Point end, double thickness)
        {
            Underlying.DrawLine(Offset(start), Offset(end), thickness);
        }

        public void DrawRectangle(Point start, Size size, double thickness, bool fill = false)
        {
            Underlying.DrawRectangle(Offset(start), size, thickness, fill);
        }

        public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            Underlying.DrawEllipse(Offset(centre), radiusX, radiusY, thickness, fill);
        }

        public void DrawPath(Point start, IList<IPathCommand> commands, double thickness, bool fill = false)
        {
            Underlying.DrawPath(Offset(start), commands, thickness, fill);
        }

        public void DrawText(Point anchor, TextAlignment alignment, IList<TextRun> textRuns)
        {
            Underlying.DrawText(Offset(anchor), alignment, textRuns);
        }

        public void Dispose()
        {
            Underlying.Dispose();
        }

        private Point Offset(Point p)
        {
            return new Point(p.X + offset.X, p.Y + offset.Y);
        }
    }
}
