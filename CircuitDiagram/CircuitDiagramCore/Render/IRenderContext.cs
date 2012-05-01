using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CircuitDiagram.Render.Path;

namespace CircuitDiagram.Render
{
    /// <summary>
    /// Provides a common interface for rendering drawing elements.
    /// </summary>
    public interface IRenderContext
    {
        /// <summary>
        /// Initializes the renderer.
        /// </summary>
        void Begin();

        /// <summary>
        /// Closes the renderer.
        /// </summary>
        void End();

        /// <summary>
        /// Starts a new section.
        /// </summary>
        /// <param name="tag">A custom object which is related to the new section.</param>
        void StartSection(object tag);

        /// <summary>
        /// Draws a line with the specified parameters.
        /// </summary>
        /// <param name="start">The point to begin the line at.</param>
        /// <param name="end">The point to end the line at.</param>
        /// <param name="thickness">The thickness of the line.</param>
        void DrawLine(Point start, Point end, double thickness);

        /// <summary>
        /// Draws a rectangle with the specified parameters.
        /// </summary>
        /// <param name="start">The top left corner of the rectangle.</param>
        /// <param name="size">The dimensions of the rectangle.</param>
        /// <param name="thickness">The thickness of the border of the rectangle.</param>
        /// <param name="fill">Whether the rectangle should be filled.</param>
        void DrawRectangle(Point start, Size size, double thickness, bool fill = false);

        /// <summary>
        /// Draws an ellipse with the specified parameters.
        /// </summary>
        /// <param name="centre">The centre point of the ellipse.</param>
        /// <param name="radiusX">The x-axis radius.</param>
        /// <param name="radiusY">The y-axis radius.</param>
        /// <param name="thickness">The thickness of the border of the ellipse.</param>
        /// <param name="fill">Whether the ellipse should be filled.</param>
        void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false);

        /// <summary>
        /// Draws a path with the specified parameters.
        /// </summary>
        /// <param name="start">The start point for the path.</param>
        /// <param name="commands">The path commands.</param>
        /// <param name="thickness">The thickness of the path stroke.</param>
        /// <param name="fill">Whether the path should be filled.</param>
        void DrawPath(Point start, IList<IPathCommand> commands, double thickness, bool fill = false);

        /// <summary>
        /// Draws a group of text runs with the specified parameters.
        /// </summary>
        /// <param name="anchor">The anchor point for the text.</param>
        /// <param name="alignment">How the text should be aligned relative to the anchor point.</param>
        /// <param name="textRuns">The text runs to render.</param>
        void DrawText(Point anchor, TextAlignment alignment, IEnumerable<CircuitDiagram.Render.TextRun> textRuns);
    }
}