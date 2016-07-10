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
using CircuitDiagram.CircuitExtensions;
using NUnit.Framework;

namespace CircuitDiagram.IO.Test.Data
{
    [TestFixture]
    public class ConnectionTest
    {
        private readonly ConnectionName connectionName = new ConnectionName("a");

        [Test]
        public void Connect()
        {
            // Create two elements
            var element1 = CreateWithConnection();
            var element2 = CreateWithConnection();

            // Connect them together
            element1.Connections[connectionName].ConnectTo(element2.Connections[connectionName]);

            // They should be connected
            Assert.That(element1.Connections[connectionName].IsConnectedTo(element2.Connections[connectionName]));
            Assert.That(element2.Connections[connectionName].IsConnectedTo(element1.Connections[connectionName]));
        }

        public void MultiConnect()
        {
            // Create three elements
            var element1 = CreateWithConnection();
            var element2 = CreateWithConnection();
            var element3 = CreateWithConnection();

            // Connect 1 and 2 together
            element1.Connections[connectionName].ConnectTo(element2.Connections[connectionName]);

            // Connect 1 and 3 togerther
            element1.Connections[connectionName].ConnectTo(element3.Connections[connectionName]);

            // 2 should now be connected to 3
            Assert.That(element2.Connections[connectionName].IsConnectedTo(element3.Connections[connectionName]));
        }

        public void Disconnect()
        {
            // Create two elements
            var element1 = CreateWithConnection();
            var element2 = CreateWithConnection();

            // Connect them together
            element1.Connections[connectionName].ConnectTo(element2.Connections[connectionName]);

            // Disconnect them
            element1.Connections[connectionName].Disconnect();

            // They should not be connected
            Assert.That(element1.Connections[connectionName].IsConnectedTo(element2.Connections[connectionName]), Is.False);
        }

        private MockElement CreateWithConnection()
        {
            var element = new MockElement();
            element.Connections.Add(connectionName, new NamedConnection(connectionName, element));
            return element;
        }
    }
}
