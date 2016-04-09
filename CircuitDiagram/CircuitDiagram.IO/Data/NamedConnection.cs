using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    public sealed class NamedConnection
    {
        internal NamedConnection(ConnectionName name, IElement owner)
        {
            Name = name;
            Owner = owner;
            Connection = new Connection(this);
        }

        public ConnectionName Name { get; }

        public IElement Owner { get; }

        public Connection Connection { get; internal set; }

        public IReadOnlyCollection<NamedConnection> ConnectedTo => Connection.NamedConnections;

        public void ConnectTo(NamedConnection other) => Connection.ConnectTo(other);

        public void Disconnect()
        {
            Connection.Disconnect(this);
            Connection = new Connection(this);
        }
    }
}
