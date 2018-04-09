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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.Document;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.View.Services
{
    class DocumentService : IDocumentService
    {
        private readonly CircuitDiagramDocumentReader reader;

        public DocumentService(CircuitDiagramDocumentReader reader)
        {
            this.reader = reader;
        }

        public CircuitDocument OpenDocument(Stream input)
        {
            var document = reader.ReadCircuit(input);

            // Convert wires to components

            var wireType = new TypeDescriptionComponentType(Guid.Parse("6353882b-5208-4f88-a83b-2271cc82b94f"), ComponentType.UnknownCollection, "wire");

            var wires = document.Wires.ToList();
            foreach (var wire in wires)
            {
                document.Elements.Remove(wire);

                var component = new PositionalComponent(wireType, wire.Layout);

                document.Elements.Add(component);
            }

            return document;
        }
    }
}
