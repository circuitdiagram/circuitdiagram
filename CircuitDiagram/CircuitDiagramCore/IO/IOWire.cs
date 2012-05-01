using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Represents a wire within a stored document format.
    /// </summary>
    public class IOWire
    {
        /// <summary>
        /// The position of the wire within the document.
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        /// The size of the wire.
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// The orientation of the wire.
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Creates a new IOWire.
        /// </summary>
        public IOWire()
        {
        }

        /// <summary>
        /// Creates a new IOWire with the specified parameters.
        /// </summary>
        /// <param name="location">The position of the wire within the document.</param>
        /// <param name="size">The size of the wire.</param>
        /// <param name="orientation">The orientation of the wire.</param>
        public IOWire(Point location, double size, Orientation orientation)
        {
            Location = location;
            Size = size;
            Orientation = orientation;
        }
    }
}
