using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.IO.Data;

namespace CircuitDiagram.IO.Test.Data
{
    class MockElement : IElement
    {
        public MockElement()
        {
            Connections = new List<NamedConnection>();
        }

        public List<NamedConnection> Connections { get; }

        IReadOnlyList<NamedConnection> IElement.Connections => Connections;
    }
}
