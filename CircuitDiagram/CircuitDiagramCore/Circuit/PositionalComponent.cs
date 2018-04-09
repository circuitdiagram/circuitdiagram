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
            : this(type, new LayoutInformation())
        {
        }

        public PositionalComponent(ComponentType type, LayoutInformation layout)
            : base(type)
        {
            Layout = layout;
        }

        /// <summary>
        /// Gets or sets the layout information for this component.
        /// </summary>
        public LayoutInformation Layout { get; set; }
    }
}
