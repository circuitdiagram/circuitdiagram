using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.Components.Description;
using CircuitDiagram.Drawing;
using CircuitDiagram.Primitives;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.Render.Connections
{
    public class ConnectionPositioner : IConnectionPositioner
    {
        public IList<ConnectionPoint> PositionConnections(PositionalComponent instance,
                                                          ComponentDescription description,
                                                          LayoutOptions layoutOptions)
        {
            var connections = new List<ConnectionPoint>();
            var flip = instance.Layout.GetFlipType();

            foreach (ConnectionGroup group in description.Connections)
            {
                if (group.Conditions.IsMet(instance, description))
                {
                    foreach (ConnectionDescription connectionDescription in group.Value)
                    {
                        Point start = connectionDescription.Start.Resolve(instance.Layout, layoutOptions);
                        start = start.WithNewX(Math.Ceiling(start.X / layoutOptions.GridSize) * layoutOptions.GridSize);
                        start = start.WithNewY(Math.Ceiling(start.Y / layoutOptions.GridSize) * layoutOptions.GridSize);

                        Point end = connectionDescription.End.Resolve(instance.Layout, layoutOptions);
                        end = end.WithNewX(Math.Floor(end.X / layoutOptions.GridSize) * layoutOptions.GridSize);
                        end = end.WithNewY(Math.Floor(end.Y / layoutOptions.GridSize) * layoutOptions.GridSize);

                        // Reverse if in the wrong order
                        bool reversed = false;
                        if ((start.X == end.X && end.Y < start.Y) || (start.Y == end.Y && end.X < start.X))
                        {
                            Point temp = start;
                            start = end;
                            end = temp;
                            reversed = true;
                        }

                        if (connectionDescription.Start.Resolve(instance.Layout, layoutOptions).X ==
                            connectionDescription.End.Resolve(instance.Layout, layoutOptions).X) // use original coordinates to check correctly when single point
                        {
                            // Vertical

                            for (double i = start.Y; i <= end.Y; i += layoutOptions.GridSize)
                            {
                                var flags = ConnectionFlags.Vertical;

                                if (!reversed)
                                {
                                    if (i == start.Y && (connectionDescription.Edge == ConnectionEdge.Start || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                    else if (i == end.Y && (connectionDescription.Edge == ConnectionEdge.End || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                }
                                else if ((flip & FlipType.Vertical) == FlipType.Vertical && reversed)
                                {
                                    if (i == start.Y && (connectionDescription.Edge == ConnectionEdge.End || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                    else if (i == end.Y && (connectionDescription.Edge == ConnectionEdge.Start || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                }

                                var location = new Point(instance.Layout.Location.X + start.X, instance.Layout.Location.Y + i);
                                connections.Add(new ConnectionPoint(location, connectionDescription.Name, flags));
                            }
                        }
                        else if (start.Y == end.Y)
                        {
                            // Horizontal

                            for (double i = start.X; i <= end.X; i += layoutOptions.GridSize)
                            {
                                ConnectionFlags flags = ConnectionFlags.Horizontal;
                                if (!reversed)
                                {
                                    if (i == start.X && (connectionDescription.Edge == ConnectionEdge.Start || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                    else if (i == end.X && (connectionDescription.Edge == ConnectionEdge.End || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                }
                                else if ((flip & FlipType.Horizontal) == FlipType.Horizontal && reversed)
                                {
                                    if (i == start.X && (connectionDescription.Edge == ConnectionEdge.End || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                    else if (i == end.X && (connectionDescription.Edge == ConnectionEdge.Start || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                }
                                
                                var location = new Point(instance.Layout.Location.X + i, instance.Layout.Location.Y + start.Y);
                                connections.Add(new ConnectionPoint(location, connectionDescription.Name, flags));
                            }
                        }
                    }
                }
            }

            return connections;
        }
    }
}
