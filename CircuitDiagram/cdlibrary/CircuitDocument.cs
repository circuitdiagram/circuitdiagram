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
        public double GridSize = 10d;

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

        #region IO
        public void Save(System.IO.Stream stream)
        {
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("circuit");
            writer.WriteAttributeString("version", "1.1");
            writer.WriteAttributeString("cd-version", "1.2");
            writer.WriteAttributeString("width", Size.Width.ToString());
            writer.WriteAttributeString("height", Size.Height.ToString());
            foreach (Component component in m_components)
            {
                writer.WriteStartElement("component");
                Dictionary<string, object> properties = new Dictionary<string, object>();
                component.Serialize(properties);
                foreach (KeyValuePair<string, object> property in properties)
                    writer.WriteAttributeString(property.Key, property.Value.ToString());

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
        }

        public DocumentLoadResult Load(System.IO.Stream stream)
        {
            XmlTextReader reader = new XmlTextReader(stream);
            this.Elements.Clear();

            bool errorOccurred = false;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Depth == 0)
                {
                    try
                    {
                        reader.MoveToAttribute("width");
                        Size.Width = double.Parse(reader.Value);
                        reader.MoveToAttribute("height");
                        Size.Height = double.Parse(reader.Value);
                        reader.MoveToElement();
                    }
                    catch (Exception)
                    {
                        errorOccurred = true;
                    }
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Depth == 1 && reader.LocalName == "component")
                {
                    try
                    {
                        reader.MoveToAttribute("type");
                        string componentType = reader.Value;
                        reader.MoveToElement();
                        Dictionary<string, object> properties = new Dictionary<string, object>();
                        reader.MoveToNextAttribute();
                        for (int i = 0; i < reader.AttributeCount; i++)
                        {
                            properties.Add(reader.Name, reader.Value);
                            reader.MoveToNextAttribute();
                        }
                        Component component = Component.Create(ComponentDataString.ConvertToString(properties));
                        reader.MoveToElement();

                        m_components.Add(component);
                    }
                    catch (Exception)
                    {
                        errorOccurred = true;
                    }
                }
            }
            reader.Close();

            foreach (Component component in Components)
                component.ApplyConnections(this);

            if (errorOccurred)
                return DocumentLoadResult.FailIncorrectFormat;
            else
                return DocumentLoadResult.Success;
        }
        #endregion

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
                        connectionCentres.Add(connection.Value.Centre);
                        connectionPoints.Add(Point.Add(connection.Key, component.Offset));
                    }
                }
            }

            foreach (Point connectionPoint in connectionPoints)
                dc.DrawEllipse(connectionPoint, 2d, 2d, 2d, true);
        }
    }
}
