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
using CircuitDiagram.Circuit;
using CircuitDiagram.Components.Description;
using CircuitDiagram.Drawing;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render.Connections;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using NUnit.Framework;

namespace CircuitDiagram.Render.Test.Render
{
    [TestFixture]
    public class ConnectionPositionerTest
    {
        private IConnectionPositioner positioner;

        [SetUp]
        public void Setup()
        {
            positioner = new ConnectionPositioner();
        }

        [Test]
        public void LayoutHorizontal()
        {
            var instance = new PositionalComponent(new TestComponentType());
            instance.Layout.Size = 100;

            var connectionGroups = new[]
            {
                new ConnectionGroup(ConditionTree.Empty, new[]
                {
                    new ConnectionDescription(new ComponentPoint("_Start"), new ComponentPoint("_Start+45x"), ConnectionEdge.Start, "Connection1"),
                })
            };

            var description = new ComponentDescription
            {
                Connections = connectionGroups
            };

            var positioned = positioner.PositionConnections(instance, description, new LayoutOptions
            {
                GridSize = 10.0
            });

            Assert.That(positioned, Has.Count.EqualTo(5));

            // First is edge
            Assert.That(positioned[0].Location, Is.EqualTo(new Point(0, 0)));
            Assert.That(positioned[0].Flags, Is.EqualTo(ConnectionFlags.Horizontal | ConnectionFlags.Edge));

            // Remaining are not edges
            for (int i = 1; i < positioned.Count; i++)
            {
                var c = positioned[i];
                Assert.That(c.Location, Is.EqualTo(new Point(i * 10, 0)));
                Assert.That(c.Flags, Is.EqualTo(ConnectionFlags.Horizontal));
            }
        }
    }
}
