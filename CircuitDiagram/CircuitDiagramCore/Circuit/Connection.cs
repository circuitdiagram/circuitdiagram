using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Circuit
{
    public class Connection : IComparable
    {
        private readonly HashSet<NamedConnection> namedConnections;

        internal Connection(NamedConnection onlyConnection)
        {
            namedConnections = new HashSet<NamedConnection>
            {
                onlyConnection
            };
        }

        internal IReadOnlyCollection<NamedConnection> NamedConnections => new ReadOnlyCollection<NamedConnection>(namedConnections.ToList());

        internal void ConnectTo(NamedConnection other)
        {
            // Add to this connection
            namedConnections.Add(other);

            // Set all connections 'other' is connected to to point to this connection
            var previousConnections = other.Connection.NamedConnections;
            foreach (var c in previousConnections)
                c.Connection = this;

            // Add all connections 'other' points to
            namedConnections.UnionWith(other.Connection.NamedConnections);

            // Set the 'other' connection to point to this
            other.Connection = this;
        }

        internal void Disconnect(NamedConnection namedConnection)
        {
            namedConnections.Remove(namedConnection);
        }
        
        public int CompareTo(object obj)
        {
            return ReferenceEquals(this, obj) ? 0 : -1;
        }
    }
}
