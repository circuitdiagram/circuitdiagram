using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.IO.Data;
using CircuitDiagram.IO.DataExtensions;
using NUnit.Framework;

namespace CircuitDiagram.IO.Test.Data
{
    [TestFixture]
    public class ConnectionTest
    {
        [Test]
        public void Connect()
        {
            // Create two elements
            var element1 = CreateWithConnection();
            var element2 = CreateWithConnection();

            // Connect them together
            element1.Connections[0].ConnectTo(element2.Connections[0]);

            // They should be connected
            Assert.That(element1.Connections[0].IsConnectedTo(element2.Connections[0]));
            Assert.That(element2.Connections[0].IsConnectedTo(element1.Connections[0]));
        }

        public void MultiConnect()
        {
            // Create three elements
            var element1 = CreateWithConnection();
            var element2 = CreateWithConnection();
            var element3 = CreateWithConnection();

            // Connect 1 and 2 together
            element1.Connections[0].ConnectTo(element2.Connections[0]);

            // Connect 1 and 3 togerther
            element1.Connections[0].ConnectTo(element3.Connections[0]);

            // 2 should now be connected to 3
            Assert.That(element2.Connections[0].IsConnectedTo(element3.Connections[0]));
        }

        public void Disconnect()
        {
            // Create two elements
            var element1 = CreateWithConnection();
            var element2 = CreateWithConnection();

            // Connect them together
            element1.Connections[0].ConnectTo(element2.Connections[0]);

            // Disconnect them
            element1.Connections[0].Disconnect();

            // They should not be connected
            Assert.That(element1.Connections[0].IsConnectedTo(element2.Connections[0]), Is.False);
        }

        private MockElement CreateWithConnection()
        {
            var element = new MockElement();
            element.Connections.Add(new NamedConnection(null, element));
            return element;
        }
    }
}
