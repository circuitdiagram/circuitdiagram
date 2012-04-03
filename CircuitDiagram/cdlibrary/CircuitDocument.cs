// CircuitDocument.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using CircuitDiagram.Components;
using System.Windows.Media;
using System.Collections.ObjectModel;
using CircuitDiagram.IO;
using CircuitDiagram.Render;
using CircuitDiagram.Elements;

namespace CircuitDiagram
{
    public class CircuitDocument
    {
        public CircuitDocumentMetadata Metadata { get; set; }

        public Size Size;

        ObservableCollection<ICircuitElement> m_elements = new ObservableCollection<ICircuitElement>();
        ObservableCollection<Component> m_components = new ObservableCollection<Component>();

        public bool SnapToGrid { get; set; }

        public bool SnapToHV { get; set; }

        public IEnumerable<ICircuitElement> Components
        {
            get { return m_elements.Where(element => element is Component); }
        }

        public ObservableCollection<ICircuitElement> Elements
        {
            get { return m_elements; }
        }

        public CircuitDocument()
        {
            SnapToGrid = true;
            SnapToHV = true;
            Size = new Size(640, 480);
        }

        private static bool AlmostEquals(double one, double two)
        {
            double result = one - two;
            return (result <= 1 && result >= -1);
        }

        public void Render(IRenderContext dc)
        {
            // Components
            foreach (Component component in Components)
                foreach (var renderDescription in component.Description.RenderDescriptions)
                    if (renderDescription.Conditions.ConditionsAreMet(component))
                        foreach (CircuitDiagram.Components.Render.IRenderCommand renderCommand in renderDescription.Value)
                            renderCommand.Render(component, dc);

            // Connections
            List<ConnectionCentre> connectionCentres = new List<ConnectionCentre>();
            List<Point> connectionPoints = new List<Point>();
            foreach (Component component in Components)
            {
                foreach (var connection in component.GetConnections())
                {
                    if (connection.Value.IsConnected && !connectionCentres.Contains(connection.Value.Centre))
                    {
                        bool draw = false;
                        if (connection.Value.ConnectedTo.Length >= 3)
                            draw = true;
                        foreach (Connection connectedConnection in connection.Value.ConnectedTo)
                        {
                            if ((connectedConnection.Flags & ConnectionFlags.Horizontal) == ConnectionFlags.Horizontal && (connection.Value.Flags & ConnectionFlags.Vertical) == ConnectionFlags.Vertical && (connection.Value.Flags & ConnectionFlags.Edge) != (connectedConnection.Flags & ConnectionFlags.Edge))
                                draw = true;
                            else if ((connectedConnection.Flags & ConnectionFlags.Vertical) == ConnectionFlags.Vertical && (connection.Value.Flags & ConnectionFlags.Horizontal) == ConnectionFlags.Horizontal && (connection.Value.Flags & ConnectionFlags.Edge) != (connectedConnection.Flags & ConnectionFlags.Edge))
                                draw = true;
                            if (draw)
                                break;
                        }
                        if (draw)
                        {
                            connectionCentres.Add(connection.Value.Centre);
                            connectionPoints.Add(Point.Add(connection.Key, component.Offset));
                        }
                    }
                }
            }

            foreach (Point connectionPoint in connectionPoints)
                dc.DrawEllipse(connectionPoint, 2d, 2d, 2d, true);
        }
    }
}
