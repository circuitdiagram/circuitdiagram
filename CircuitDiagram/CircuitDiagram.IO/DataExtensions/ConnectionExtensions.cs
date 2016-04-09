using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.IO.Data;

namespace CircuitDiagram.IO.DataExtensions
{
    public static class ConnectionExtensions
    {
        public static bool IsConnectedTo(this NamedConnection connection, NamedConnection other)
        {
            return connection.ConnectedTo.Contains(other);
        }
    }
}
