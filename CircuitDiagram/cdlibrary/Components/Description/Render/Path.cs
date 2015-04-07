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
using System.Text.RegularExpressions;
using CircuitDiagram.Render.Path;

namespace CircuitDiagram.Components.Description.Render
{
    public class RenderPath : IRenderCommand
    {
        public ComponentPoint Start { get; set; }
        public double Thickness { get; set; }
        public bool Fill { get; set; }
        public IList<IPathCommand> Commands { get; set; }

        public RenderCommandType Type
        {
            get { return RenderCommandType.Path; }
        }

        public RenderPath()
        {
            Start = new ComponentPoint();
            Thickness = 2d;
            Fill = false;
            Commands = new List<IPathCommand>();
        }

        public RenderPath(ComponentPoint start, double thickness, bool fill, IList<IPathCommand> commands)
        {
            Start = start;
            Thickness = thickness;
            Fill = fill;
            Commands = commands;
        }

        public void Render(Component component, CircuitDiagram.Render.IRenderContext dc, bool absolute)
        {
            IList<IPathCommand> commands = Commands;
            if (component.IsFlipped == true)
            {
                commands = new List<IPathCommand>(Commands.Count);
                foreach (IPathCommand command in Commands)
                    commands.Add(command.Flip(component.Orientation == Orientation.Horizontal));
            }
            if (absolute)
                dc.DrawPath(Point.Add(Start.Resolve(component), component.Location), commands, Thickness, Fill);
            else
                dc.DrawPath(Start.Resolve(component), commands, Thickness, Fill);
        }

        public static RenderPath Parse(string start, string data, double thickness = 2d, bool fill = false)
        {
            List<IPathCommand> pathCommands = PathHelper.ParseCommands(data);

            return new RenderPath(new ComponentPoint(start), thickness, fill, pathCommands);
        }
    }
}
