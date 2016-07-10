using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Render.Connections
{
    public class ConnectionVisualiser : IConnectionVisualiser
    {
        public bool VisualiseConnections(ICollection<ConnectionPoint> connections)
        {
            // Not a connection
            if (connections.Count < 2)
                return false;

            // An edge and a non-edge of different orientations
            if (connections.Any(x => x.IsEdge &&
                                     connections.Any(y => !y.IsEdge &&
                                                          x.Orientation != y.Orientation)))
                return true;

            // Three edges containing two orientations
            return connections.Any(x => x.IsEdge &&
                                        connections.Any(y => y != x && y.IsEdge &&
                                                             connections.Any(z => z != x &&
                                                                                  z != y &&
                                                                                  z.IsEdge &&
                                                                                  (x.Orientation != y.Orientation ||
                                                                                   x.Orientation != z.Orientation ||
                                                                                   y.Orientation != z.Orientation))));
        }
    }
}
