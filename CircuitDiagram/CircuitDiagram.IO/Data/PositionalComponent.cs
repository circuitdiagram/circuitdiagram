using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    public class PositionalComponent : Component, IPositionalElement
    {
        public PositionalComponent(ComponentType type,
                                   ComponentConfiguration configuration,
                                   ElementLocation location)
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
