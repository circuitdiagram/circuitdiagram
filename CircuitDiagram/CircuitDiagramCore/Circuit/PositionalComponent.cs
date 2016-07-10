using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Circuit
{
    /// <summary>
    /// Represents an element in a circuit that has it's own behaviour, a defined set of properties
    /// and layout information.
    /// </summary>
    public class PositionalComponent : Component, IPositionalConnectedElement
    {
        public PositionalComponent(ComponentType type)
            : base(type)
        {
            Layout = new LayoutInformation(new Point(0, 0));
        }

        public PositionalComponent(ComponentType type,
                                   ComponentConfiguration configuration,
                                   Point location)
            : base(type, configuration)
        {
            Layout = new LayoutInformation(location);
        }

        /// <summary>
        /// Gets or sets the layout information for this component.
        /// </summary>
        public LayoutInformation Layout { get; }
    }
}
