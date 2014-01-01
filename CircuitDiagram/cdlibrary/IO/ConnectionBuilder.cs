// ConnectionBuilder.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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
using CircuitDiagram.Components;
using CircuitDiagram.Components.Description;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Provides methods for manipulating connections between components.
    /// </summary>
    public static class ConnectionHelper
    {
        /// <summary>
        /// Removes wires from the connections within a document, connecting components on either end of the wire.
        /// </summary>
        /// <param name="document">The document containing the connections to convert.</param>
        /// <returns>A dictionary containing all components and their associated connections in the format Name-ConnectionID.</returns>
        public static Dictionary<Component, Dictionary<string, string>> RemoveWires(CircuitDocument document)
        {
            
            // Remove wires
            foreach (Component wire in document.Components.Where(c => ComponentHelper.IsWire(c)))
            {
                // Get list of all connections for this wire
                var connections = wire.GetConnectedConnections();

                // If no connections, skip
                if (connections.Count() == 0)
                    continue;

                // Choose a centre to connect everything to
                var centre = connections.First().Centre;

                // For each connection, remove the wire itself
                foreach (var connection in connections.Where(conn => conn.Centre != centre))
                {
                    // This centre will be merged with 'centre' above
                    var tempCentre = connection.Centre;

                    // Disconnect this wire
                    //connection.Disconnect();

                    // If no longer a centre, skip
                    if (connection.Centre == null)
                        continue;

                    // Join 'tempCentre' to 'centre'
                    foreach (var moveConnection in tempCentre.Connected)
                    {
                        moveConnection.SetCentre(centre);
                        centre.Connected.Add(moveConnection);
                    }
                }
            }
            
            // Join ConnectionCentre's for the same connection name on a component
            foreach (Component component in document.Components.Where(c => !ComponentHelper.IsWire(c)))
            {
                Dictionary<string, List<ConnectionCentre>> connectionNames = new Dictionary<string,List<ConnectionCentre>>();

                foreach (var connection in component.GetConnectedConnections().Where(conn => conn.IsConnected))
                {
                    if (!connectionNames.ContainsKey(connection.Description.Name))
                        connectionNames.Add(connection.Description.Name, new List<ConnectionCentre>());

                    if (!connectionNames[connection.Description.Name].Contains(connection.Centre))
                        connectionNames[connection.Description.Name].Add(connection.Centre);
                }

                // Join all ConnectionCentre's with same name
                foreach (var connectionName in connectionNames)
                {
                    // The centres will be merged with this one
                    var centre = connectionName.Value.First();

                    foreach (var oldCentre in connectionName.Value.Skip(1))
                    {
                        foreach (Connection moveConnection in oldCentre.Connected)
                        {
                            moveConnection.SetCentre(centre);
                            centre.Connected.Add(moveConnection);
                        }
                    }
                }
            }

            // Assign an ID to each ConnectionCentre
            int connectionIdCounter = 0;
            var connectionIDs = new Dictionary<ConnectionCentre, string>();
            foreach (Component component in document.Components.Where(c => !ComponentHelper.IsWire(c)))
            {
                foreach (var connection in component.GetConnectedConnections().Where(conn => conn.IsConnected))
                {
                    if (!connectionIDs.ContainsKey(connection.Centre))
                        connectionIDs.Add(connection.Centre, (connectionIdCounter++).ToString());
                }
            }

            // Create return dictionary
            var returnDict = new Dictionary<Component, Dictionary<string, string>>();

            // Create dictionary of connections for each component
            foreach (Component component in document.Components.Where(c => !ComponentHelper.IsWire(c)))
            {
                var connectionsDict = new Dictionary<string, string>();

                foreach (var connection in component.GetConnectedConnections())
                    if (!connectionsDict.ContainsKey(connection.Description.Name))
                        connectionsDict.Add(connection.Description.Name, connectionIDs[connection.Centre]);

                returnDict.Add(component, connectionsDict);
            }

            // Restore normal connections
            foreach (Component component in document.Components)
                component.ResetConnections();
            foreach (Component component in document.Components)
                component.ApplyConnections(document);

            return returnDict;
        }

        /// <summary>
        /// Represents a <see cref="Component"/> and its <see cref="Description"/>.
        /// </summary>
        class UniqueConnectionDescription
        {
            public Component Component;
            public ConnectionDescription ConnectionDescription;

            public UniqueConnectionDescription(Component component, ConnectionDescription connectionDescription)
            {
                Component = component;
                ConnectionDescription = connectionDescription;
            }
        }

        /// <summary>
        /// Connects two <see cref="ConnectionCentre"/>s together.
        /// </summary>
        /// <param name="destination">The ConnectionCentre to merge to.</param>
        /// <param name="b">The ConnectionCentre that will be made redundant.</param>
        static void JoinConnectionCentres(ConnectionCentre destination, ConnectionCentre b)
        {
            while (b.Connected.Count > 0)
            {
                b.Connected[0].ConnectTo(destination.Connected.First());
            }
        }
    }
}
