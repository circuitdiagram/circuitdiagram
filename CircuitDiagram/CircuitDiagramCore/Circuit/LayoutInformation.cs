using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Circuit
{
    /// <summary>
    /// Describes the position of an element within a document.
    /// </summary>
    public class LayoutInformation
    {
        public LayoutInformation()
        {
            Location = new Point();
        }

        /// <summary>
        /// Gets the position of the component within the document.
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        /// Gets or sets the size of the component.
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the component is flipped.
        /// </summary>
        public bool IsFlipped { get; set; }

        /// <summary>
        /// Gets or sets the orientation of the component.
        /// </summary>
        public Orientation Orientation { get; set; }
    }
}
