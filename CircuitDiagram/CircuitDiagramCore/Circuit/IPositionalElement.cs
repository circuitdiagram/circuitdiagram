using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Circuit
{
    /// <summary>
    /// An element within a document that has layout information.
    /// </summary>
    public interface IPositionalElement : IElement
    {
        /// <summary>
        /// Gets or sets the layout information for this positional element.
        /// </summary>
        LayoutInformation Layout { get; }
    }
}
