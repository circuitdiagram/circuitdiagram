// Path.cs
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
using System.Windows.Media;

namespace CircuitDiagram.Components.Render.Path
{
    public class Path : IRenderCommand
    {
        public ComponentPoint Start { get; set; }
        public double Thickness { get; set; }
        public bool Fill { get; set; }
        private IList<IPathCommand> m_commands;

        public IList<IPathCommand> Commands
        {
            get { return m_commands; }
        }

        public RenderCommandType Type
        {
            get { return RenderCommandType.Path; }
        }

        public Path()
        {
            Start = new ComponentPoint();
            Thickness = 2d;
            Fill = false;
            m_commands = new List<IPathCommand>();
        }

        public Path(ComponentPoint start, double thickness, bool fill, IList<IPathCommand> commands)
        {
            Start = start;
            Thickness = thickness;
            Fill = fill;
            m_commands = commands;
        }

        public void Render(Component component, CircuitDiagram.Render.IRenderContext dc)
        {
            IList<IPathCommand> commands = Commands;
            if (component.IsFlipped)
            {
                commands = new List<IPathCommand>(Commands.Count);
                foreach (IPathCommand command in Commands)
                    commands.Add(command.Flip(component.Horizontal));
            }
            if (dc.Absolute)
                dc.DrawPath(Point.Add(Start.Resolve(component), new Vector(component.Offset.X, component.Offset.Y)), commands, Thickness, Fill);
            else
                dc.DrawPath(Start.Resolve(component), commands, Thickness, Fill);
        }
    }
}
