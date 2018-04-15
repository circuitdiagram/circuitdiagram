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
using CircuitDiagram.Primitives;
using CircuitDiagram.Render.Connections;
using NUnit.Framework;

namespace CircuitDiagram.Render.Test.Render.Connections
{
    [TestFixture]
    public class ConnectionVisualiserTest
    {
        private ConnectionVisualiser connectionVisualiser;

        [SetUp]
        public void Setup()
        {
            connectionVisualiser = new ConnectionVisualiser(null);
        }

        [Test]
        public void EdgeNonEdge()
        {
            var connections = new[]
            {
                new ConnectionPoint(new Point(0, 0), "a", ConnectionFlags.Edge | ConnectionFlags.Vertical),
                new ConnectionPoint(new Point(0, 0), "a", ConnectionFlags.Horizontal),
            };

            bool visualise = connectionVisualiser.VisualiseConnection(connections);
            Assert.That(visualise, Is.True);
        }

        [Test]
        public void Single()
        {
            var connections = new[]
            {
                new ConnectionPoint(new Point(0, 0), "a", ConnectionFlags.Edge | ConnectionFlags.Vertical),
            };

            bool visualise = connectionVisualiser.VisualiseConnection(connections);
            Assert.That(visualise, Is.False);
        }

        [Test]
        public void TwoEdges()
        {
            var connections = new[]
            {
                new ConnectionPoint(new Point(0, 0), "a", ConnectionFlags.Edge | ConnectionFlags.Vertical),
                new ConnectionPoint(new Point(0, 0), "a", ConnectionFlags.Edge | ConnectionFlags.Horizontal),
            };

            bool visualise = connectionVisualiser.VisualiseConnection(connections);
            Assert.That(visualise, Is.False);
        }

        [Test]
        public void ThreeEdges()
        {
            var connections = new[]
            {
                new ConnectionPoint(new Point(0, 0), "a", ConnectionFlags.Edge | ConnectionFlags.Vertical),
                new ConnectionPoint(new Point(0, 0), "a", ConnectionFlags.Edge | ConnectionFlags.Horizontal),
                new ConnectionPoint(new Point(0, 0), "a", ConnectionFlags.Edge | ConnectionFlags.Horizontal),
            };

            bool visualise = connectionVisualiser.VisualiseConnection(connections);
            Assert.That(visualise, Is.True);
        }
    }
}
