using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Render.Connections
{
    /// <summary>
    /// Determines whether a connection should be shown when rendering a circuit.
    /// </summary>
    public interface IConnectionVisualiser
    {
        /// <summary>
        /// Determines whether a connection should be shown for the supplied connections at the same point
        /// on a diagram.
        /// </summary>
        /// <param name="connections"></param>
        /// <returns></returns>
        bool VisualiseConnections(ICollection<ConnectionPoint> connections);
    }
}
