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
    public class PropertiesReaderTest
    {
        private PropertiesReader reader;
        private ReaderContext context;
        private CircuitDiagramDocument document;

        [SetUp]
        public void Setup()
        {
            reader = new PropertiesReader();
            context = new ReaderContext();
            document = new CircuitDiagramDocument();
        }

        [Test]
        public void WarnUnableToParseInt()
        {
            var xml = new XElement(Ns.Document + "properties",
                new XElement(Ns.Document + "width", "non-int"),
                new XElement(Ns.Document + "height", 1));

            reader.ReadProperties(document, xml, context);

            Assert.That(context.Warnings, Has.Count.EqualTo(1));
            Assert.That(context.Warnings[0].Code, Is.EqualTo(ReaderErrorCodes.UnableToParseValueAsInt));
        }

        [Test]
        public void WarnMultipleElements()
        {
            var xml = new XElement(Ns.Document + "properties",
                new XElement(Ns.Document + "width", 1),
                new XElement(Ns.Document + "width", 2),
                new XElement(Ns.Document + "height", 1));

            reader.ReadProperties(document, xml, context);

            Assert.That(context.Warnings, Has.Count.EqualTo(1));
            Assert.That(context.Warnings[0].Code, Is.EqualTo(ReaderErrorCodes.DuplicateElement));
        }

        [Test]
        public void SetDocumentProperties()
        {
            var xml = new XElement(Ns.Document + "properties",
                new XElement(Ns.Document + "width", 1),
                new XElement(Ns.Document + "height", 2));

            reader.ReadProperties(document, xml, context);

            Assert.That(context.Warnings, Is.Empty);
            Assert.That(document.Size.Width, Is.EqualTo(1));
            Assert.That(document.Size.Height, Is.EqualTo(2));
        }
    }
}
