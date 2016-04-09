using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    /// <summary>
    /// Describes the position of an element within a document.
    /// </summary>
    public class LayoutInformation
    {
        internal LayoutInformation(ComponentLocation location)
        {
            Location = location;
        }

        /// <summary>
        /// Gets or sets the position of the component within the document.
        /// </summary>
        public ComponentLocation Location { get; }

        /// <summary>
        /// Gets or sets the size of the component.
        /// If null, the component cannot be resized.
        /// </summary>
        public double? Size { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the component is flipped.
        /// If null, the component cannot be flipped.
        /// </summary>
        public bool? IsFlipped { get; set; }

        /// <summary>
        /// Gets or sets the orientation of the component.
        /// If null, the component does not have an associated orientation.
        /// </summary>
        public ComponentOrientation? Orientation { get; set; }
    }
}
