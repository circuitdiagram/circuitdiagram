using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Circuit
{
    public interface IConnectedElement : IElement
    {
        IDictionary<ConnectionName, NamedConnection> Connections { get; }
    }
}
