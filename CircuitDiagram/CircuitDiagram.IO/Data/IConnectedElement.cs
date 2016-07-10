using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    public interface IConnectedElement : IElement
    {
        IReadOnlyDictionary<ConnectionName, NamedConnection> Connections { get; }
    }
}
