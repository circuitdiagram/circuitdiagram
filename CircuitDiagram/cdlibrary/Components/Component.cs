// Component.cs
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
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Reflection;
using System.Globalization;
using CircuitDiagram.Components.Render;
using CircuitDiagram.Elements;

namespace CircuitDiagram.Components
{
    public class Component : ICircuitElement
    {
        public event EventHandler Updated;

        public static Component Create(ComponentDescription description, Dictionary<string, object> data)
        {
            Component newComponent = new Component(description);

            // Load other properties
            newComponent.Deserialize(data);

            newComponent.ResetConnections();

            newComponent.SetEditorEnumValues();

            return newComponent;
        }

        public static Component Create(string data)
        {
            // Load data
            data = data.Replace(",", "\r\n");
            Dictionary<string, object> properties = ComponentDataString.ConvertToDictionary(data);

            // Find component description
            ComponentDescription description;
            if (properties.ContainsKey("@rid"))
                description = ComponentHelper.FindDescriptionByRuntimeID(int.Parse(properties["@rid"].ToString()));
            else if (properties.ContainsKey("@guid"))
                description = ComponentHelper.FindDescription(new Guid(properties["@guid"].ToString()));
            else
                description = ComponentHelper.FindDescription(properties["@type"].ToString());
            Component newComponent = new Component(description);

            // Apply configuration
            if (properties.ContainsKey("@config"))
            {
                ComponentConfiguration configuration = description.Metadata.Configurations.FirstOrDefault(item => item.Name == properties["@config"].ToString());
                if (configuration != null)
                {
                    foreach (KeyValuePair<string, object> setter in configuration.Setters)
                    {
                        if (!properties.ContainsKey(setter.Key))
                            properties.Add(setter.Key, setter.Value);
                        else
                            properties[setter.Key] = setter.Value;
                    }
                }        
            }

            // Load other properties
            newComponent.Deserialize(properties);

            newComponent.ResetConnections();

            newComponent.SetEditorEnumValues();

            return newComponent;
        }

        #region Properties
        public ComponentDescription Description { get; set; }
        public bool IsFlipped { get; set; }

        public double Size { get; set; }

        public Point StartLocation { get { return new Point(); } }

        private Vector m_offset;
        public Vector Offset
        {
            get { return m_offset; }
            set
            {
                m_offset = value;
                InvalidateVisual();
            }
        }

        public bool Horizontal { get; set; }

        private Dictionary<ComponentProperty, object> m_propertyValues { get; set; }

        public IComponentEditor Editor { get; private set; }

        private Dictionary<Point, Connection> m_connections = new Dictionary<Point, Connection>();
        #endregion

        private Component(ComponentDescription description)
        {
            Description = description;
            IsFlipped = false;
            m_propertyValues = new Dictionary<ComponentProperty, object>(description.Properties.Length);
            foreach (ComponentProperty property in description.Properties)
                m_propertyValues.Add(property, property.Default);
            if (System.Threading.Thread.CurrentThread.GetApartmentState() == System.Threading.ApartmentState.STA)
            {
                if (ComponentHelper.CreateEditor != null)
                    Editor = ComponentHelper.CreateEditor(this);

                if (Editor != null && ComponentHelper.ComponentUpdatedDelegate != null)
                    Editor.ComponentUpdated += ComponentHelper.ComponentUpdatedDelegate;
            }
        }

        private void SetEditorEnumValues()
        {
#warning TIDY

            //if (Editor == null)
            //    return;

            //foreach (ComponentProperty property in this.Description.Properties)
            //{
            //    if (property.Type == typeof(string) && property.EnumOptions != null)
            //    {
            //        ComboBox propertyEditControl = this.Editor.EditorControls[property] as ComboBox;
            //        if (propertyEditControl != null)
            //        {
            //            propertyEditControl.SelectedItem = GetProperty(property) as string;
            //        }
            //    }
            //}
        }

        public ComponentProperty FindProperty(string name)
        {
            string searchFor = name.Replace("$", "").ToLower();

            foreach (ComponentProperty property in Description.Properties)
            {
                if (property.Name.ToLower() == searchFor)
                    return property;
            }
            return null;
        }

        public ComponentProperty FindPropertyBySerializedName(string serializedName)
        {
            foreach (ComponentProperty dProperty in Description.Properties)
                if (dProperty.SerializedName == serializedName)
                    return dProperty;
            return null;
        }

        public object GetProperty(ComponentProperty property)
        {
            if (m_propertyValues.ContainsKey(property))
                return m_propertyValues[property];
            else
                return null;
        }

        public string GetFormattedProperty(ComponentProperty property)
        {
            if (m_propertyValues.ContainsKey(property))
                return property.Format(this, m_propertyValues[property]);
            else
                return null;
        }

        public void SetProperty(ComponentProperty property, object value)
        {
            if (m_propertyValues.ContainsKey(property))
            {
                // cast to correct type
                if (property.Type == value.GetType())
                    m_propertyValues[property] = value;
                else if (property.Type == typeof(double))
                    m_propertyValues[property] = double.Parse(value.ToString());
                else if (property.Type == typeof(int))
                    m_propertyValues[property] = int.Parse(value.ToString());
                else if (property.Type == typeof(bool))
                    m_propertyValues[property] = bool.Parse(value.ToString());
            }
        }

        public void ResetConnections()
        {
            foreach (KeyValuePair<Point, Connection> connection in m_connections)
                connection.Value.Disconnect();
            m_connections.Clear();
            foreach (ConnectionGroup group in Description.Connections)
            {
                if (group.Conditions.ConditionsAreMet(this))
                {
                    foreach (ConnectionDescription connectionDescription in group.Value)
                    {
                        Point start = connectionDescription.Start.Resolve(this);
                        start.X = Math.Ceiling(start.X / ComponentHelper.GridSize) * ComponentHelper.GridSize;
                        start.Y = Math.Ceiling(start.Y / ComponentHelper.GridSize) * ComponentHelper.GridSize;
                        Point end = connectionDescription.End.Resolve(this);
                        end.X = Math.Floor(end.X / ComponentHelper.GridSize) * ComponentHelper.GridSize;
                        end.Y = Math.Floor(end.Y / ComponentHelper.GridSize) * ComponentHelper.GridSize;

                        // Reverse if in the wrong order
                        bool reversed = false;
                        if ((start.X == end.X && end.Y < start.Y) || (start.Y == end.Y && end.X < start.X))
                        {
                            Point temp = start;
                            start = end;
                            end = temp;
                            reversed = true;
                        }

                        if (start.X == end.X)
                        {
                            for (double i = start.Y; i <= end.Y; i += ComponentHelper.GridSize)
                            {
                                ConnectionFlags flags = ConnectionFlags.Vertical;
                                if (!reversed)
                                {
                                    if (i == start.Y && (connectionDescription.Edge == ConnectionEdge.Start || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                    else if (i == end.Y && (connectionDescription.Edge == ConnectionEdge.End || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                }
                                else if (!Horizontal && reversed)
                                {
                                    if (i == start.Y && (connectionDescription.Edge == ConnectionEdge.End || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                    else if (i == end.Y && (connectionDescription.Edge == ConnectionEdge.Start || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                }
                                Point key = new Point(start.X, i);
                                if (!m_connections.ContainsKey(key))
                                    m_connections.Add(key, new Connection(this, flags, connectionDescription));
                            }
                        }
                        else if (start.Y == end.Y)
                        {
                            for (double i = start.X; i <= end.X; i += ComponentHelper.GridSize)
                            {
                                ConnectionFlags flags = ConnectionFlags.Horizontal;
                                if (!reversed)
                                {
                                    if (i == start.X && (connectionDescription.Edge == ConnectionEdge.Start || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                    else if (i == end.X && (connectionDescription.Edge == ConnectionEdge.End || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                }
                                else if (Horizontal && reversed)
                                {
                                    if (i == start.X && (connectionDescription.Edge == ConnectionEdge.End || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                    else if (i == end.X && (connectionDescription.Edge == ConnectionEdge.Start || connectionDescription.Edge == ConnectionEdge.Both))
                                        flags |= ConnectionFlags.Edge;
                                }
                                Point key = new Point(i, start.Y);
                                if (!m_connections.ContainsKey(key))
                                m_connections.Add(key, new Connection(this, flags, connectionDescription));
                            }
                        }
                    }
                }
            }
        }

        public void DisconnectConnections()
        {
            foreach (KeyValuePair<Point, Connection> connection in m_connections)
                connection.Value.Disconnect();
        }

        public void ApplyConnections(CircuitDocument document)
        {
            foreach (Component component in document.Components)
            {
                if (component == this)
                    continue;
                foreach (KeyValuePair<Point, Connection> connection in this.GetConnections())
                {
                    double thisX = this.Offset.X + connection.Key.X;
                    double thisY = this.Offset.Y + connection.Key.Y;

                    foreach (KeyValuePair<Point, Connection> connection2 in component.GetConnections())
                    {
                        double otherX = component.Offset.X + connection2.Key.X;
                        double otherY = component.Offset.Y + connection2.Key.Y;

                        if (thisX == otherX && thisY == otherY && (!connection2.Value.IsConnected || !connection2.Value.ConnectedTo.Contains(connection.Value)))
                        {
                            //if (((connection.Value.Flags & ConnectionFlags.Edge) == ConnectionFlags.Edge && (connection2.Value.Flags & ConnectionFlags.Edge) != ConnectionFlags.Edge) || ((connection.Value.Flags & ConnectionFlags.Edge) != ConnectionFlags.Edge && ((connection2.Value.Flags & ConnectionFlags.Edge) == ConnectionFlags.Edge)))
                            if ((connection.Value.Flags & ConnectionFlags.Edge) == ConnectionFlags.Edge || (connection2.Value.Flags & ConnectionFlags.Edge) == ConnectionFlags.Edge)
                                connection.Value.ConnectTo(connection2.Value);
                        }
                    }
                }
            }
        }

        public Connection GetConnection(Point point)
        {
            foreach (KeyValuePair<Point, Connection> pair in m_connections)
            {
                if (pair.Key.Equals(point))
                    return pair.Value;
            }
            return null;
        }

        public IEnumerable<KeyValuePair<Point, Connection>> GetConnections()
        {
            return m_connections;
        }

        public IEnumerable<Connection> GetConnectedConnections()
        {
            return m_connections.Values.Where(connection => connection.IsConnected);
        }

        public void Layout(double x, double y, double size, bool horizontal, bool flipped)
        {
            this.Offset = new Vector(x, y);
            this.Horizontal = horizontal;
            this.Size = size;
            this.IsFlipped = flipped;

            this.ResetConnections();
            this.InvalidateVisual();
        }

        public void Serialize(Dictionary<string, object> properties)
        {
            // add common properties
            properties.Add("@type", this.Description.ComponentName);
            if (!ComponentHelper.IsStandardComponent(this.Description) && this.Description.Metadata.GUID != Guid.Empty)
                properties.Add("@guid", this.Description.Metadata.GUID);
            properties.Add("@x", this.Offset.X);
            properties.Add("@y", this.Offset.Y);
            properties.Add("@orientation", (Horizontal ? "horizontal" : "vertical"));
            if (Description.CanResize)
                properties.Add("@size", Size);
            if (Description.CanFlip)
                properties.Add("@flipped", IsFlipped);

            foreach (KeyValuePair<ComponentProperty, object> property in m_propertyValues)
            {
                if (property.Key.OtherConditions.ContainsKey(PropertyOtherConditionType.Serialize) && !property.Key.OtherConditions[PropertyOtherConditionType.Serialize].ConditionsAreMet(this))
                    continue;
                properties.Add(property.Key.SerializedName, property.Value);
            }
        }

        private static object GetAsCorrectType(Type type, object value)
        {
            if (type.IsAssignableFrom(value.GetType()))
                return value;

            if (type == typeof(double))
            {
                return double.Parse(value.ToString());
            }
            if (type == typeof(int))
                return int.Parse(value.ToString());
            if (type.IsEnum)
            {
                return int.Parse(value.ToString());
            }
            if (type == typeof(bool))
            {
                return bool.Parse(value.ToString());
            }

            return value;
        }

        public void Deserialize(Dictionary<string, object> properties)
        {
            this.Horizontal = true;
            foreach (KeyValuePair<string, object> property in properties)
            {
                // load common properties
                if (property.Key == "@x")
                    this.Offset = new Vector((double)GetAsCorrectType(typeof(double), property.Value), this.Offset.Y);
                else if (property.Key == "@y")
                    this.Offset = new Vector(this.Offset.X, (double)GetAsCorrectType(typeof(double), property.Value));
                else if (property.Key == "@orientation" && property.Value.ToString().ToLower() == "vertical")
                    this.Horizontal = false;
                else if (property.Key == "@size")
                    this.Size = (double)GetAsCorrectType(typeof(double), property.Value);
                else if (property.Key == "@flipped" && property.Value.ToString().ToLower() == "true")
                    IsFlipped = true;
                else
                {
                    // custom property
                    foreach (ComponentProperty dProperty in Description.Properties)
                    {
                        if (dProperty.SerializedName == property.Key)
                        {
                            SetProperty(dProperty, property.Value);
                            break;
                        }
                    }
                }
            }
        }

        public void Render(CircuitDiagram.Render.IRenderContext dc)
        {
            foreach (RenderDescription renderDescription in Description.RenderDescriptions)
            {
                if (renderDescription.Conditions.ConditionsAreMet(this))
                    renderDescription.Render(this, dc);
            }
        }

        public override string ToString()
        {
            return "type: " + Description.ComponentName;
        }

        public void InvalidateVisual()
        {
            if (Updated != null)
                Updated(this, new EventArgs());
        }
    }
}
