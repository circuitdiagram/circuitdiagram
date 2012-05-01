using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram
{
    /// <summary>
    /// Represents either horizontal or vertical.
    /// </summary>
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    public static class OrientationExtensions
    {
        /// <summary>
        /// Reverses the orientation.
        /// </summary>
        /// <param name="orientation">The orientation to reverse.</param>
        /// <returns>The opposite orientation to that given.</returns>
        public static Orientation Reverse(this Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
                return Orientation.Vertical;
            else
                return Orientation.Horizontal;
        }
    }
}
