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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Primitives;
using NUnit.Framework;

namespace CircuitDiagram.Document.Test
{
    [TestFixture]
    public class CircuitDiagramDocumentReaderTest
    {
        private CircuitDiagramDocument document;

        [SetUp]
        public void SetUp()
        {
            var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().First(x => x.EndsWith("Data.TestCircuit.cddx"));

            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (var ms = new MemoryStream())
            {
                resourceStream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);

                var reader = new CircuitDiagramDocumentReader();
                document = reader.ReadCircuit(ms);
            }
        }

        [Test]
        public void ReadsProperties()
        {
            Assert.That(document.Size.Width, Is.EqualTo(640));
            Assert.That(document.Size.Height, Is.EqualTo(480));
        }

        [Test]
        public void ReadsComponents()
        {
            Assert.That(document.Components.Count(), Is.EqualTo(2));

            var c0 = document.Components.ElementAt(0);
            Assert.That(c0.Type.CollectionItem, Is.EqualTo("resistor"));

            var c1 = document.Components.ElementAt(1);
            Assert.That(c1.Type.CollectionItem, Is.EqualTo("cell"));
        }

        [Test]
        public void ReadsWires()
        {
            Assert.That(document.Wires.Count(), Is.EqualTo(2));

            var w0 = document.Wires.ElementAt(0);
            Assert.That(w0.Layout.Location.X, Is.EqualTo(100.0));
            Assert.That(w0.Layout.Location.Y, Is.EqualTo(100.0));
            Assert.That(w0.Layout.Size, Is.EqualTo(100.0));
            Assert.That(w0.Layout.IsFlipped, Is.False);
            Assert.That(w0.Layout.Orientation, Is.EqualTo(Orientation.Vertical));

            var w1 = document.Wires.ElementAt(1);
            Assert.That(w1.Layout.Location.X, Is.EqualTo(280.0));
            Assert.That(w1.Layout.Location.Y, Is.EqualTo(100.0));
            Assert.That(w1.Layout.Size, Is.EqualTo(100.0));
            Assert.That(w1.Layout.IsFlipped, Is.False);
            Assert.That(w1.Layout.Orientation, Is.EqualTo(Orientation.Vertical));
        }
    }
}
