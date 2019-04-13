// This file is part of Circuit Diagram.
// Copyright (c) 2017 Samuel Fisher
//  
// Circuit Diagram is free software; you can redistribute it and/or
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
// along with Circuit Diagram. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render.Path;

namespace CircuitDiagram.Render
{
    public class TranslationDrawingContext : IDrawingContext
    {
        private readonly Vector offset;
        private readonly IDrawingContext underlying;

        public TranslationDrawingContext(Vector offset, IDrawingContext underlying)
        {
            this.offset = offset;
            this.underlying = underlying;
        }

        public void DrawLine(Point start, Point end, double thickness)
        {
            underlying.DrawLine(start.Add(offset), end.Add(offset), thickness);
        }

        public void DrawRectangle(Point start, Size size, double thickness, bool fill = false)
        {
            underlying.DrawRectangle(start.Add(offset), size, thickness, fill);
        }

        public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            underlying.DrawEllipse(centre.Add(offset), radiusX, radiusY, thickness, fill);
        }

        public void DrawPath(Point start, IList<IPathCommand> commands, double thickness, bool fill = false)
        {
            underlying.DrawPath(start.Add(offset), commands, thickness, fill);
        }

        public void DrawText(Point anchor, TextAlignment alignment, IList<TextRun> textRuns)
        {
            underlying.DrawText(anchor.Add(offset), alignment, textRuns);
        }

        public void Dispose()
        {
            underlying.Dispose();
        }
    }
}
