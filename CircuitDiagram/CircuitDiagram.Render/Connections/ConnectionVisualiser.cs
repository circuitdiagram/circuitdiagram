// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
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
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.CircuitExtensions;
using CircuitDiagram.Drawing;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Render.Connections
{
    public class ConnectionVisualiser : IConnectionVisualiser
    {
        private static readonly ComponentType WireType = new TypeDescriptionComponentType(Guid.Parse("6353882b-5208-4f88-a83b-2271cc82b94f"), new Uri("uri:internal"), "wire");

        private readonly IComponentDescriptionLookup descriptionLookup;
        private readonly IConnectionPositioner connectionPositioner;

        public ConnectionVisualiser(IComponentDescriptionLookup descriptionLookup)
        {
            this.descriptionLookup = descriptionLookup;
            connectionPositioner = new ConnectionPositioner();
        }

        public IList<VisualisedConnection> PositionConnections(CircuitDocument document, LayoutOptions layoutOptions)
        {
            foreach (var wire in document.Wires().ToList())
            {
                document.Elements.Remove(wire);
                document.Elements.Add(WireToComponent(wire));
            }

            // Compute connections
            var connectionPoints = document.PositionalComponents().SelectMany(x => GetConnectionPoints(x, layoutOptions).Select(c => Tuple.Create(x, c)));
            var connectionsByLocation = connectionPoints.GroupBy(x => x.Item2.Location).Where(x => x.Any(c => c.Item2.IsEdge) && x.Count() > 1).ToList();

            // Connect components
            foreach (var c in connectionsByLocation)
            {
                foreach (var c1 in c)
                {
                    foreach (var c2 in c)
                    {
                        var connection1 = c1.Item1.Connections.GetConnection(c1.Item2.Connection, c1.Item1);
                        var connection2 = c2.Item1.Connections.GetConnection(c2.Item2.Connection, c2.Item1);
                        connection1.ConnectTo(connection2);
                    }
                }
            }

            // Return the connections that the connectionVisualiser indicates should be rendered
            var points = connectionsByLocation
                         .Select(x => new VisualisedConnection
                         {
                             Connection = x.Select(c => c.Item1.Connections[c.Item2.Connection].Connection).First(),
                             Location = x.Key,
                             Render = VisualiseConnection(x.Select(y => y.Item2).ToList())
                         }).ToList();

            foreach (var wire in document.PositionalComponents().Where(x => x.Type == WireType).ToList())
            {
                document.Elements.Remove(wire);
                document.Elements.Add(ComponentToWire(wire));
            }

            return points;
        }

        private IList<ConnectionPoint> GetConnectionPoints(PositionalComponent component, LayoutOptions layoutOptions)
        {
            var description = descriptionLookup.GetDescription(component.Type);

            if (description == null)
            {
                throw new ApplicationException($"No component description available for {component.Type}");
            }

            return connectionPositioner.PositionConnections(component, description, layoutOptions);
        }

        private static PositionalComponent WireToComponent(Wire w)
        {
            return new PositionalComponent(WireType, w.Layout);
        }

        private static Wire ComponentToWire(PositionalComponent c)
        {
            return new Wire(c.Layout);
        }

        /// <summary>
        /// Determines whether a connection should be rendered for the specified connction points.
        /// </summary>
        internal bool VisualiseConnection(ICollection<ConnectionPoint> connectionPoints)
        {
            // Not a connection
            if (connectionPoints.Count < 2)
                return false;

            // An edge and a non-edge of different orientations
            if (connectionPoints.Any(x => x.IsEdge &&
                                          connectionPoints.Any(y => !y.IsEdge &&
                                                                    x.Orientation != y.Orientation)))
                return true;

            // Three edges containing two orientations
            return connectionPoints.Any(x => x.IsEdge &&
                                             connectionPoints.Any(y => y != x && y.IsEdge &&
                                                                       connectionPoints.Any(z => z != x &&
                                                                                                 z != y &&
                                                                                                 z.IsEdge &&
                                                                                                 (x.Orientation != y.Orientation ||
                                                                                                  x.Orientation != z.Orientation ||
                                                                                                  y.Orientation != z.Orientation))));
        }
    }
}
