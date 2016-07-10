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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CircuitDiagram.Document.InternalReader;
using CircuitDiagram.Document.ReaderErrors;
using NUnit.Framework;
using Ns = CircuitDiagram.Document.Namespaces;

namespace CircuitDiagram.Document.Test.InternalReader
{
    [TestFixture]
    public class DefinitionsReaderTest
    {
        private DefinitionsReader reader;
        private ReaderContext context;
        private CircuitDiagramDocument document;

        [SetUp]
        public void Setup()
        {
            reader = new DefinitionsReader();
            context = new ReaderContext();
            document = new CircuitDiagramDocument();
        }

        [Test]
        public void WarnUnableToParseUri()
        {
            var xml = new XElement(Ns.Document + "definitions",
                new XElement(Ns.Document + "src",
                    new XAttribute("col", "not-a-uri!"),
                    new XElement(Ns.Document + "add",
                        new XAttribute("id", "0"),
                        new XAttribute("item", "component"))));

            reader.ReadDefinitions(document, xml, context);

            Assert.That(context.Warnings, Has.Count.EqualTo(1));
            Assert.That(context.Warnings[0].Code, Is.EqualTo(ReaderErrorCodes.UnableToParseValueAsUri));
        }

        [Test]
        public void RegistersComponentTypes()
        {
            var xml = new XElement(Ns.Document + "definitions",
                new XElement(Ns.Document + "src",
                    new XAttribute("col", "http://component/collection"),
                    new XElement(Ns.Document + "add",
                        new XAttribute("id", "0"),
                        new XAttribute("item", "component"))));

            reader.ReadDefinitions(document, xml, context);

            Assert.That(context.Warnings, Is.Empty);
            Assert.That(context.GetComponentType("0"), Is.Not.Null);
            Assert.That(context.GetComponentType("0").CollectionItem.Item, Is.EqualTo("component"));
        }
    }
}
