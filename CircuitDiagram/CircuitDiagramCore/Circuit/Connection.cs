// This file is part of Circuit Diagram.
// http://www.circuit-diagram.org/
// 
// Copyright (c) 2019 Samuel Fisher
//  
// Circuit Diagram is free software; you can redistribute it and/or
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Circuit
{
    public class Connection : IEquatable<Connection>
    {
        private readonly HashSet<NamedConnection> namedConnections;

        internal Connection(NamedConnection onlyConnection, string connectionId)
        {
            namedConnections = new HashSet<NamedConnection>
            {
                onlyConnection
            };
            ConnectionId = connectionId;
        }

        public string ConnectionId { get; set; }

        internal IReadOnlyCollection<NamedConnection> NamedConnections => new ReadOnlyCollection<NamedConnection>(namedConnections.ToList());

        internal void ConnectTo(NamedConnection other)
        {
            // Unset the connection ID if the other connection has a different ID
            if (other.Connection.ConnectionId != null && other.Connection.ConnectionId != ConnectionId)
            {
                ConnectionId = null;
            }

            // Add all connections 'other' points to
            namedConnections.UnionWith(other.Connection.NamedConnections);

            // Set all connections 'other' is connected to to point to this connection
            var previousConnections = other.Connection.NamedConnections;
            foreach (var c in previousConnections)
                c.Connection = this;
        }

        internal void Disconnect(NamedConnection namedConnection)
        {
            namedConnections.Remove(namedConnection);
        }

        public bool Equals(Connection other)
        {
            return ReferenceEquals(this, other);
        }
    }
}
