// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CircuitDiagram.Circuit;
using CircuitDiagram.Document.ReaderErrors;

namespace CircuitDiagram.Document.InternalReader
{
    class ReaderContext
    {
        private readonly List<ErrorInstance> warnings;
        private readonly Dictionary<string, ComponentType> componentTypes;
        private readonly Dictionary<string, IList<NamedConnection>> connections;

        public ReaderContext()
        {
            warnings = new List<ErrorInstance>();
            componentTypes = new Dictionary<string, ComponentType>();
            connections = new Dictionary<string, IList<NamedConnection>>();
        }

        public void Log(ReaderErrorCode errorCode, IXmlLineInfo line, string parameter = null)
        {
            warnings.Add(new ErrorInstance(errorCode, line.LineNumber, line.LinePosition, parameter));
        }

        public IReadOnlyList<ErrorInstance> Warnings => warnings;

        public void RegisterComponentType(string id, ComponentType type)
        {
            componentTypes.Add(id, type);
        }

        public ComponentType GetComponentType(string id)
        {
            ComponentType type;
            if (componentTypes.TryGetValue(id, out type))
                return type;
            return null;
        }

        public void ApplyConnection(string id, NamedConnection connection)
        {
            IList<NamedConnection> existing;
            if (!connections.TryGetValue(id, out existing))
            {
                connections.Add(id, new List<NamedConnection>
                {
                    connection
                });
                return;
            }

            foreach (var c in existing)
                connection.ConnectTo(c);
            existing.Add(connection);
        }
    }
}
