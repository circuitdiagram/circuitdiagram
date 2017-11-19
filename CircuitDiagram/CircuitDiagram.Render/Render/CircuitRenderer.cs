using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.Components.Description;
using CircuitDiagram.Drawing;
using CircuitDiagram.Primitives;
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

        public CircuitRenderer(IComponentDescriptionLookup descriptionLookup)
        {
            this.descriptionLookup = descriptionLookup;
        }

        public void RenderCircuit(CircuitDocument circuit, IDrawingContext drawingContext)
        {
            foreach (var component in circuit.Components
                                             .Where(c => c is PositionalComponent).Cast<PositionalComponent>())
                RenderComponent(component, drawingContext, ignoreOffset: false);

            foreach (var wire in circuit.Wires)
                RenderWire(wire, drawingContext);
        }

        public virtual void RenderComponent(PositionalComponent component, IDrawingContext drawingContext, bool ignoreOffset = true)
        {
            var description = descriptionLookup.GetDescription(component.Type);
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
            var size = wire.Layout.Orientation == Orientation.Horizontal ?
                new Point(wire.Layout.Size, 0.0) : new Point(0.0, wire.Layout.Size);
            drawingContext.DrawLine(wire.Layout.Location, Point.Add(wire.Layout.Location, size), 2.0);
        }

        private static string GetFormattedVariable(string variableName, PositionalComponent instance, ComponentDescription description)
        {
            string propertyName = variableName.Substring(1);
            return description.Properties.First(x => x.Name == propertyName)
                              .Format(instance, description, description.GetProperty(instance, propertyName));
        }
    }
}
