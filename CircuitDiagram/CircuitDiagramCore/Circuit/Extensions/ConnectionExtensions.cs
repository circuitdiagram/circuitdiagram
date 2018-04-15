using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;

namespace CircuitDiagram.CircuitExtensions
{
    public static class ConnectionExtensions
    {
        public static bool IsConnectedTo(this NamedConnection connection, NamedConnection other)
        {
            return connection == other || connection.ConnectedTo.Contains(other);
        }

        public static NamedConnection GetConnection(this IDictionary<ConnectionName, NamedConnection> connections, ConnectionName connectionName, IConnectedElement owner)
        {
            if (connections.TryGetValue(connectionName, out var namedConnection))
                return namedConnection;

            namedConnection = new NamedConnection(connectionName, owner);
            connections[connectionName] = namedConnection;
            return namedConnection;
        }
    }
}
