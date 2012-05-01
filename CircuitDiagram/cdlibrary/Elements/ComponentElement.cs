using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CircuitDiagram.Elements
{
    /// <summary>
    /// Represents a component which cannot be shown or modified.
    /// </summary>
    public class DisabledComponent
    {
        /// <summary>
        /// The collection this component belongs to.
        /// </summary>
        public string ImplementationCollection { get; set; }
        /// <summary>
        /// The item within the collection this component belongs to.
        /// </summary>
        public string ImplementationItem { get; set; }

        /// <summary>
        /// The location of the component within the document.
        /// </summary>
        public Vector? Location { get; set; }
        /// <summary>
        /// The size of the component.
        /// </summary>
        public double? Size { get; set; }
        /// <summary>
        /// Whether the component is flipped or not.
        /// </summary>
        public bool? IsFlipped { get; set; }
        /// <summary>
        /// Whether the component horizontal.
        /// </summary>
        public Orientation? Orientation { get; set; }

        /// <summary>
        /// Component properties.
        /// </summary>
        public Dictionary<string, object> Properties { get; private set; }

        public DisabledComponent()
        {
            Properties = new Dictionary<string, object>();
        }

        public DisabledComponent(string implementationCollection, string implmementationItem, Vector location, double size, bool isFlipped, Orientation orientation)
        {
            Properties = new Dictionary<string, object>();
            ImplementationCollection = implementationCollection;
            ImplementationItem = implmementationItem;
            Location = location;
            Size = size;
            IsFlipped = isFlipped;
            Orientation = orientation;
        }

        public DisabledComponent(string implementationCollection, string implmementationItem, Vector location, double size, bool isFlipped, Orientation orientation, Dictionary<string, object> properties)
        {
            ImplementationCollection = implementationCollection;
            ImplementationItem = implmementationItem;
            Location = location;
            Size = size;
            IsFlipped = isFlipped;
            Orientation = orientation;
            Properties = properties;
        }
    }
}
