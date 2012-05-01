using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.IO;

namespace CircuitDiagram.Render.Path
{
    /// <summary>
    /// Represents a single element within a path.
    /// </summary>
    public interface IPathCommand
    {
        /// <summary>
        /// The end point for this path command.
        /// </summary>
        Point End { get; }

        /// <summary>
        /// Type of path command.
        /// </summary>
        CommandType Type { get; }

        /// <summary>
        /// Gets the SVG path syntax for the path command.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="previous"></param>
        /// <returns></returns>
        string Shorthand(Point offset, Point previous);

        /// <summary>
        /// Flips the path along the specified axis.
        /// </summary>
        /// <param name="horizontal">Whether to flip along the horizontal axis.</param>
        /// <returns>A flipped copy of the path command.</returns>
        IPathCommand Flip(bool horizontal);
    }
}
