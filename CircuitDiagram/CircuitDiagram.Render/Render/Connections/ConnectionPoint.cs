using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.Components.Description;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Render.Connections
{
    /// <summary>
    /// Represents an instance of a connection at a specified point on a diagram.
    /// </summary>
    public class ConnectionPoint
    {
        public ConnectionPoint(Point location, ConnectionName connection, ConnectionFlags flags)
        {
            Location = location;
            Connection = connection;
            Flags = flags;
        }

        /// <summary>
        /// Gets the position of this connection relative to the component it belongs to.
        /// </summary>
        public Point Location { get; }

        public ConnectionName Connection { get; }

        /// <summary>
        /// Gets the flags for this point.
        /// </summary>
        public ConnectionFlags Flags { get; }

        /// <summary>
        /// Gets a value indicating whethter this connection point is an edge connection.
        /// </summary>
        public bool IsEdge => (Flags & ConnectionFlags.Edge) == ConnectionFlags.Edge;

        /// <summary>
        /// Gets the orientation of the connection.
        /// </summary>
        public Orientation Orientation => (Flags & ConnectionFlags.Horizontal) != 0 ? Orientation.Horizontal : Orientation.Vertical;
    }
}
