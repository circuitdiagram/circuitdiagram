using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Circuit
{
    [DebuggerDisplay("{GetHashCode()}: {Name.Value} ({Owner.GetHashCode()})")]
    public sealed class NamedConnection
    {
        public NamedConnection(ConnectionName name, IConnectedElement owner)
        {
            Name = name;
            Owner = owner;
            Connection = new Connection(this);
        }

        public ConnectionName Name { get; }

        public IConnectedElement Owner { get; }

        public Connection Connection { get; internal set; }

        public IEnumerable<NamedConnection> ConnectedTo => Connection.NamedConnections.Except(new [] { this });

        public void ConnectTo(NamedConnection other) => Connection.ConnectTo(other);

        public void Disconnect()
        {
            Connection.Disconnect(this);
            Connection = new Connection(this);
        }
    }
}
