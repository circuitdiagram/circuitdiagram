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
            // Remove wires from connections
            foreach (Component component in document.Components)
            {
                if (component.Description.ComponentName.ToLower() == "wire")
                {
                    ConnectionCentre destination = null;

                    foreach (var connection in component.GetConnectedConnections())
                    {
                        if (destination == null)
                        {
                            destination = connection.Centre;
                            connection.Centre.Connected.Remove(connection);
                        }
                        else
                        {
                            if (destination.Connected.Count < 0)
                                continue;
                            ConnectionCentre b = connection.Centre;
                            connection.Centre.Connected.Remove(connection);
                            JoinConnectionCentres(destination, connection.Centre);
                        }
                    }
                }
            }

            // Build list of connections
            Dictionary<Component, Dictionary<string, string>> t1 = new Dictionary<Component, Dictionary<string, string>>();

            List<UniqueConnectionDescription> allUniqueConnectionDescriptions = new List<UniqueConnectionDescription>();
            Dictionary<UniqueConnectionDescription, List<Connection>> connectRef = new Dictionary<UniqueConnectionDescription, List<Connection>>();
            foreach (Component component in document.Components)
            {
                Dictionary<ConnectionDescription, UniqueConnectionDescription> processed = new Dictionary<ConnectionDescription, UniqueConnectionDescription>();
                foreach (KeyValuePair<System.Windows.Point, Connection> connection in component.GetConnections())
                {
                    if (!processed.ContainsKey(connection.Value.Description))
                    {
                        UniqueConnectionDescription a = new UniqueConnectionDescription(component, connection.Value.Description);
                        processed.Add(connection.Value.Description, a);
                        allUniqueConnectionDescriptions.Add(a);
                        connectRef.Add(a, new List<Connection>());
                    }
                    connectRef[processed[connection.Value.Description]].Add(connection.Value);
                }
            }

            List<List<UniqueConnectionDescription>> collectionY = new List<List<UniqueConnectionDescription>>();
            foreach (UniqueConnectionDescription x in allUniqueConnectionDescriptions)
            {
                bool breakAll = false;

                foreach (List<UniqueConnectionDescription> y in collectionY)
                {
                    foreach (Connection connectionX in connectRef[x])
                    {
                        foreach (UniqueConnectionDescription y1 in y)
                        {
                            if (x == y1)
                                break;

                            foreach (Connection connectionY in connectRef[y1])
                            {
                                if (connectionY.IsConnected && connectionX.IsConnected && connectionY.Centre == connectionX.Centre)
                                {
                                    y.Add(x);
                                    breakAll = true;
                                    break;
                                }
                            }

                            if (breakAll)
                                break;
                        }

                        if (breakAll)
                            break;
                    }

                    if (breakAll)
                        break;
                }

                if (!breakAll)
                {
                    List<UniqueConnectionDescription> nl = new List<UniqueConnectionDescription>();
                    nl.Add(x);
                    collectionY.Add(nl);
                }
            }

            // Asign an ID to each connection
            int namedConnectionRefCounter = 0;
            foreach (var item in collectionY)
            {
                foreach (var item2 in item)
                {
                    if (!t1.ContainsKey(item2.Component))
                        t1.Add(item2.Component, new Dictionary<string, string>());
                    if (item.Count > 1)
                        t1[item2.Component].Add(item2.ConnectionDescription.Name, namedConnectionRefCounter.ToString());
                    namedConnectionRefCounter++;
                }
            }

            // Restore normal connections
            foreach (Component component in document.Components)
                component.ResetConnections();
            foreach (Component component in document.Components)
                component.ApplyConnections(document);

            return t1;
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
