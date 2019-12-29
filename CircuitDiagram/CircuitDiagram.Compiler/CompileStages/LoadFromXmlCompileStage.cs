// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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

using System.IO;
using System.Linq;
using CircuitDiagram.Components.Description;
using CircuitDiagram.IO;
using CircuitDiagram.TypeDescriptionIO.Xml;
using CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Definitions;

namespace CircuitDiagram.Compiler.CompileStages
{
    class LoadFromXmlCompileStage : ICompileStage
    {
        public void Run(CompileContext context)
        {
            XmlLoader loader = new XmlLoader();
            loader.UseDefinitions();

            // TODO: Add errors to context
            if (!loader.Load(context.Input, out var description))
                return;

            // The component XML format doesn't provide an ID, so make one now
            description.ID = "C0";

            context.Description = description;
        }
    }
}
