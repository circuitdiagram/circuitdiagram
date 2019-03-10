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
using CircuitDiagram.Components.Description;
using CircuitDiagram.Drawing;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render.Connections;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;

namespace CircuitDiagram.Render
{
    /// <summary>
    /// Renders a <see cref="CircuitDocument" /> by matching the <see cref="ComponentType"/> of each
    /// component with a <see cref="ComponentDescription"/>.
    /// </summary>
    public class CircuitRenderer : ICircuitRenderer
    {
        private readonly IComponentDescriptionLookup descriptionLookup;
        private readonly IConnectionVisualiser connectionVisualiser;

        public CircuitRenderer(IComponentDescriptionLookup descriptionLookup)
            : this(descriptionLookup, new ConnectionVisualiser(descriptionLookup))
        {
        }

        public CircuitRenderer(IComponentDescriptionLookup descriptionLookup, IConnectionVisualiser connectionVisualiser)
        {
            this.descriptionLookup = descriptionLookup;
            this.connectionVisualiser = connectionVisualiser;
        }

        public void RenderCircuit(CircuitDocument circuit, IDrawingContext drawingContext)
        {
            foreach (var component in circuit.PositionalComponents())
                RenderComponent(component, drawingContext, ignoreOffset: false);

            foreach (var wire in circuit.Wires())
                RenderWire(wire, drawingContext);

            var connections = connectionVisualiser.PositionConnections(circuit, new LayoutOptions() {GridSize = 10.0});
            foreach (var connection in connections.Where(x => x.Render).Select(x => x.Location))
                RenderConnection(connection, drawingContext);
        }

        public virtual void RenderComponent(PositionalComponent component, IDrawingContext drawingContext, bool ignoreOffset = true)
        {
            var description = descriptionLookup.GetDescription(component.Type);
            if (description == null)
                throw new ApplicationException($"No component description available for {component.Type}");

            var flags = description.DetermineFlags(component);

            var layoutOptions = new LayoutOptions
            {
                Absolute = !ignoreOffset,
                AlignMiddle = (flags & FlagOptions.MiddleMustAlign) == FlagOptions.MiddleMustAlign,
                GridSize = 10.0
            };

            var layoutContext = new LayoutContext(layoutOptions, p => GetFormattedVariable(p, component, description));

            foreach (var renderDescription in description.RenderDescriptions)
            {
                if (renderDescription.Conditions.IsMet(component, description))
                    renderDescription.Render(component, layoutContext, drawingContext);
            }
        }

        public virtual void RenderWire(Wire wire, IDrawingContext drawingContext)
        {
            var size = wire.Layout.Orientation == Orientation.Horizontal ? new Point(wire.Layout.Size, 0.0) : new Point(0.0, wire.Layout.Size);
            drawingContext.DrawLine(wire.Layout.Location, Point.Add(wire.Layout.Location, size), 2.0);
        }

        public virtual void RenderConnection(Point connection, IDrawingContext drawingContext)
        {
            drawingContext.DrawEllipse(connection, 2d, 2d, 2d, true);
        }

        private static string GetFormattedVariable(string variableName, PositionalComponent instance, ComponentDescription description)
        {
            string propertyName = variableName.Substring(1);
            return description.Properties.First(x => x.Name == propertyName)
                              .Format(instance, description, description.GetProperty(instance, propertyName));
        }
    }
}
