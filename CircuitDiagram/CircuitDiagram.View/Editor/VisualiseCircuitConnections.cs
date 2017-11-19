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
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.Drawing;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render;
using CircuitDiagram.Render.Connections;

namespace CircuitDiagram.View.Editor
{
    class VisualiseCircuitConnections
    {
        private readonly IComponentDescriptionLookup descriptionLookup;
        private readonly IConnectionPositioner connectionPositioner;
        private readonly IConnectionVisualiser connectionVisualiser;

        public VisualiseCircuitConnections(IComponentDescriptionLookup descriptionLookup,
                                           IConnectionPositioner connectionPositioner,
                                           IConnectionVisualiser connectionVisualiser)
        {
            this.descriptionLookup = descriptionLookup;
            this.connectionPositioner = connectionPositioner;
            this.connectionVisualiser = connectionVisualiser;
        }

        public IList<Point> VisualiseConnections(IEnumerable<PositionalComponent> components, LayoutOptions layoutOptions)
        {
            // Compute connections
            var connectionPoints = components.SelectMany(x => GetConnectionPoints(x, layoutOptions).Select(c => Tuple.Create(x, c)));
            var connectionsByLocation = connectionPoints.GroupBy(x => x.Item2.Location).Where(x => x.Any(c => c.Item2.IsEdge) && x.Count() > 1).ToList();

            // Connect components
            foreach (var c in connectionsByLocation)
            {
                foreach (var c1 in c)
                {
                    foreach (var c2 in c)
                    {
                        var connection1 = c1.Item1.Connections[c1.Item2.Connection];
                        var connection2 = c2.Item1.Connections[c2.Item2.Connection];
                        connection1.ConnectTo(connection2);
                    }
                }
            }

            // Return the connections that the connectionVisualiser indicates should be rendered
            return connectionsByLocation
                .Where(x => connectionVisualiser.VisualiseConnections(x.Select(y => y.Item2).ToList()))
                .Select(x => x.Key).ToList();
        }

        private IList<ConnectionPoint> GetConnectionPoints(PositionalComponent component, LayoutOptions layoutOptions)
        {
            var description = descriptionLookup.GetDescription(component.Type);
            return connectionPositioner.PositionConnections(component, description, layoutOptions);
        }
    }
}
