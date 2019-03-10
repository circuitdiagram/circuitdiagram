// This file is part of Circuit Diagram.
// http://www.circuit-diagram.org/
// 
// Copyright (c) 2019 Samuel Fisher
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
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Circuit
{
    public static class CircuitDocumentExtensions
    {
        public static IEnumerable<Component> Components(this IReadOnlyCircuitDocument document) => document.Elements.Where(el => el is Component).Cast<Component>();

        public static IEnumerable<PositionalComponent> PositionalComponents(this IReadOnlyCircuitDocument document) => document.Elements.Where(el => el is PositionalComponent).Cast<PositionalComponent>();

        public static IEnumerable<Wire> Wires(this IReadOnlyCircuitDocument document) => document.Elements.Where(el => el is Wire).Cast<Wire>();

        public static IEnumerable<IPositionalElement> PositionalElements(this IReadOnlyCircuitDocument document) => document.Elements.Where(el => el is IPositionalElement).Cast<IPositionalElement>();
    }
}
