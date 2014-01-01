// Connection.cs
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

using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components
{
    public class Connection
    {
        private ConnectionCentre m_connectionCentre;

        public ConnectionDescription Description { get; private set; }
        public ConnectionFlags Flags { get; private set; }
        public Component Owner { get; private set; }
        public ConnectionCentre Centre { get { return m_connectionCentre; } }
        public Connection[] ConnectedTo { get { return (m_connectionCentre != null ? m_connectionCentre.Connected.ToArray() : null); } }

        public bool IsConnected
        {
            get { return m_connectionCentre != null; }
        }

        public Connection(Component owner, ConnectionFlags flags, ConnectionDescription description)
        {
            Owner = owner;
            Flags = flags;
            Description = description;
        }

        public void ConnectTo(Connection connection)
        {
            Disconnect();

            if (connection.m_connectionCentre != null)
            {
                m_connectionCentre = connection.m_connectionCentre;
                m_connectionCentre.Connected.Add(this);
            }
            else
            {
                m_connectionCentre = new ConnectionCentre();
                m_connectionCentre.Connected.Add(this);
                connection.m_connectionCentre = m_connectionCentre;
                m_connectionCentre.Connected.Add(connection);
            }
        }

        public void Disconnect()
        {
            if (m_connectionCentre != null)
            {
                m_connectionCentre.Connected.Remove(this);
                if (m_connectionCentre.Connected.Count == 1)
                {
                    m_connectionCentre.Connected[0].m_connectionCentre = null;
                    m_connectionCentre.Connected.Clear();
                }
                m_connectionCentre = null;
            }
        }

        internal void SetCentre(ConnectionCentre centre)
        {
            m_connectionCentre = centre;
        }
    }

    [Flags]
    public enum ConnectionFlags
    {
        Horizontal = 1,
        Vertical = 2,
        Edge = 4
    }
}
