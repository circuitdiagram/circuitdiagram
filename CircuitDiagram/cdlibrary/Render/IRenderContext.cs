// IRenderContext.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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
using System.Linq;
using System.Text;
using System.Windows;
using CircuitDiagram.Components.Render.Path;

namespace CircuitDiagram.Render
{
    public interface IRenderContext
    {
        bool Absolute { get; }

        void Begin();
        void End();

        void DrawLine(Point start, Point end, double thickness);
        void DrawRectangle(Point start, Size size, double thickness, bool fill = false);
        void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false);
        void DrawPath(Point start, IList<IPathCommand> commands, double thickness, bool fill = false);
        void DrawText(Point anchor, CircuitDiagram.Components.Render.TextAlignment alignment, IEnumerable<CircuitDiagram.Components.Render.TextRun> textRuns);
    }
}