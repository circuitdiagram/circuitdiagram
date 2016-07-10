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
using System.Xml;
using System.Xml.Linq;
using Ns = CircuitDiagram.Document.Namespaces;


namespace CircuitDiagram.Document.InternalReader
{
    class MainDocumentReader
    {
        private readonly PropertiesReader propertiesReader;
        private readonly DefinitionsReader definitionsReader;
        private readonly ElementsReader elementsReader;

        public MainDocumentReader()
        {
            propertiesReader = new PropertiesReader();
            definitionsReader = new DefinitionsReader();
            elementsReader = new ElementsReader();
        }

        public void Read(CircuitDiagramDocument document, Stream stream)
        {
            var context = new ReaderContext();

            var xml = XDocument.Load(XmlReader.Create(stream));
            var root = xml.Root;

            var propertiesEl = (from el in root.Elements()
                                where el.Name == Ns.Document + "properties"
                                select el).Single();
            propertiesReader.ReadProperties(document, propertiesEl, context);

            var definitionsEl = (from el in root.Elements()
                                 where el.Name == Ns.Document + "definitions"
                                 select el).Single();
            definitionsReader.ReadDefinitions(document, definitionsEl, context);

            var elementsEl = (from el in root.Elements()
                              where el.Name == Ns.Document + "elements"
                              select el).Single();
            elementsReader.ReadElements(document, elementsEl, context);
        }
    }
}
