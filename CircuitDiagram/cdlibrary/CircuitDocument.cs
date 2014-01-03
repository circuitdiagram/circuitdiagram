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
    /// <summary>
    /// A collection of components representing a circuit diagram.
    /// </summary>
    public class CircuitDocument
    {
        /// <summary>
        /// The document metadata.
        /// </summary>
        public CircuitDocumentMetadata Metadata { get; set; }

        /// <summary>
        /// Dimensions of the document.
        /// </summary>
        public Size Size;

        ObservableCollection<ICircuitElement> m_elements = new ObservableCollection<ICircuitElement>();

        /// <summary>
        /// Gets the circuit elements which are components.
        /// </summary>
        public IEnumerable<Component> Components
        {
            get { return m_elements.Where(element => element is Component).Cast<Component>(); }
        }

        /// <summary>
        /// Gets all circuit elements contained within the document.
        /// </summary>
        public ObservableCollection<ICircuitElement> Elements
        {
            get { return m_elements; }
        }

        /// <summary>
        /// A list of components which cannot be shown because no visual representation is available.
        /// </summary>
        public List<DisabledComponent> DisabledComponents { get; set; }

        public CircuitDocument()
        {
            Size = new Size(640, 480);
            DisabledComponents = new List<DisabledComponent>();
            Metadata = new CircuitDocumentMetadata();
        }

        /// <summary>
        /// Renders the document using the specified renderer.
        /// </summary>
        /// <param name="dc">The renderer to use.</param>
        public void Render(IRenderContext dc)
        {
            // Render components
            foreach (Component component in Components)
                foreach (var renderDescription in component.Description.RenderDescriptions)
                    if (renderDescription.Conditions.IsMet(component))
                        foreach (CircuitDiagram.Components.Description.Render.IRenderCommand renderCommand in renderDescription.Value)
                            renderCommand.Render(component, dc, true);

            // Determine connections
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
                            connectionPoints.Add(Point.Add(connection.Key, component.Location));
                        }
                    }
                }
            }

            // Render connections
            foreach (Point connectionPoint in connectionPoints)
                dc.DrawEllipse(connectionPoint, 2d, 2d, 2d, true);
        }

        /// <summary>
        /// Updates the list of components to be embedded when the document is saved.
        /// </summary>
        public void UpdateEmbedComponents()
        {
            // Remove old items
            List<EmbedDescription> toRemove = new List<EmbedDescription>();
            foreach (EmbedDescription description in Metadata.EmbedComponents)
            {
                bool found = false;
                foreach (Component component in Components)
                    if (component.Description == description.Description)
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    toRemove.Add(description);
            }
            foreach(EmbedDescription removeItem in toRemove)
                Metadata.EmbedComponents.Remove(removeItem);

            // Add new items
            foreach (Component component in Components)
            {
                if (component.Description == ComponentHelper.WireDescription)
                    continue; // skip wires

                if (Metadata.EmbedComponents.FirstOrDefault(item => item.Description == component.Description) == null)
                    Metadata.EmbedComponents.Add(new EmbedDescription() { Description = component.Description, IsEmbedded = ComponentHelper.ShouldEmbedDescription(component.Description) });
            }
        }
    }
}
