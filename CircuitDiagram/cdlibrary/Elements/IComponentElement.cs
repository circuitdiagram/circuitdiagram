using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CircuitDiagram.Elements
{
    /// <summary>
    /// Represents a visible component within a circuit document.
    /// </summary>
    public interface IComponentElement : ICircuitElement
    {
        /// <summary>
        /// The collection this component belongs to.
        /// </summary>
        string ImplementationCollection { get; }
        /// <summary>
        /// The item within the collection this component belongs to.
        /// </summary>
        string ImplementationItem { get; }
        /// <summary>
        /// The size of the component.
        /// </summary>
        double Size { get; }
        /// <summary>
        /// Whether the component is flipped.
        /// </summary>
        bool IsFlipped { get; }
        /// <summary>
        /// Whether the component is horizontal.
        /// </summary>
        Orientation Orientation { get; }
        /// <summary>
        /// Properties of the component.
        /// </summary>
        IDictionary<string, object> Properties { get; }
    }
}
