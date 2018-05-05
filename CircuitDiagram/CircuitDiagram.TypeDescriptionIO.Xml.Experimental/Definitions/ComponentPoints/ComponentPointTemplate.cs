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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using CircuitDiagram.Primitives;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Experimental.Definitions.ComponentPoints
{
    /// <summary>
    /// Represents a <see cref="ComponentPoint"/> that contains variables.
    /// A <see cref="ComponentPoint"/> can be constructed by supplying values for the variables.
    /// </summary>
    public class ComponentPointTemplate
    {
        public ComponentPointTemplate(ComponentPosition xPosition,
                                      ComponentPosition yPosition,
                                      IReadOnlyList<IModifierToken> xModifiers,
                                      IReadOnlyList<IModifierToken> yModifiers)
        {
            XPosition = xPosition;
            YPosition = yPosition;
            XModifiers = xModifiers;
            YModifiers = yModifiers;
            XVariables = xModifiers.Where(t => t is DefinitionModifierToken).Cast<DefinitionModifierToken>().Select(t => t.Name);
            YVariables = yModifiers.Where(t => t is DefinitionModifierToken).Cast<DefinitionModifierToken>().Select(t => t.Name);
            Variables = XVariables.Concat(YVariables);
        }

        public ComponentPosition XPosition { get; }

        public ComponentPosition YPosition { get; }

        public IReadOnlyList<IModifierToken> XModifiers { get; }

        public IReadOnlyList<IModifierToken> YModifiers { get; }
        
        public IEnumerable<string> XVariables { get; }

        public IEnumerable<string> YVariables { get; }

        public IEnumerable<string> Variables { get; }

        public ComponentPoint Construct(IReadOnlyDictionary<string, double> definitions)
        {
            var xOffset = Construct(XModifiers, definitions);
            var yOffset = Construct(YModifiers, definitions);

            return new ComponentPoint(XPosition, YPosition, new Vector(xOffset, yOffset));
        }

        private static double Construct(IReadOnlyList<IModifierToken> modifiers, IReadOnlyDictionary<string, double> definitions)
        {
            return modifiers.Aggregate(0.0, (agg, next) =>
            {
                switch (next)
                {
                    case ConstantModifierToken constant:
                        return agg + constant.Offset;
                    case DefinitionModifierToken variable:
                        return agg + (variable.Negated ? -1 : 1) * definitions[variable.Name];
                    default:
                        throw new NotSupportedException();
                }
            });
        }
    }

    public interface IModifierToken
    {
    }

    public class ConstantModifierToken : IModifierToken
    {
        public double Offset { get; set; }
    }

    public class DefinitionModifierToken : IModifierToken
    {
        public bool Negated { get; set; }

        public string Name { get; set; }
    }
}
