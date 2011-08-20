// CircuitDocument.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2011  Sam Fisher
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
using System.Windows.Media;
using System.Windows;
using System.Xml;

namespace CircuitDiagram
{
    public class CircuitDocument
    {
        public double GridSize = 10d;
        public event EventHandler VisualInvalidated;

        public Size Size;

        List<EComponent> m_components = new List<EComponent>();
        List<EComponent> m_tempComponents = new List<EComponent>();

        public EComponent SelectedComponent { get; set; }
        public bool SnapToGrid { get; set; }
        public bool SnapToHV { get; set; }
        public List<EComponent> Components
        {
            get
            {
                if (VisualInvalidated != null)
                    VisualInvalidated(this, new EventArgs());
                UpdateJoins();
                return m_components;
            }
        }
        public List<EComponent> TempComponents
        {
            get
            {
                if (VisualInvalidated != null)
                    VisualInvalidated(this, new EventArgs());
                return m_tempComponents;
            }
        }

        public CircuitDocument()
        {
            SnapToGrid = true;
            SnapToHV = true;
            Size = new Size(640, 480);
        }

        public void UpdateComponent(EComponent component)
        {
            // reverse points if necessary
            Point newStart = component.StartLocation;
            Point newEnd = component.EndLocation;
            bool switched = false;
            if (component.StartLocation.X < component.EndLocation.X)
            {
                newStart = component.EndLocation;
                newEnd = component.StartLocation;
                switched = true;
            }

            if (this.SnapToGrid) // snap to grid
            {
                if (Math.IEEERemainder(newStart.X, 20d) != 0)
                    newStart.X = ComponentHelper.Snap(newStart, this.GridSize).X;
                if (Math.IEEERemainder(newStart.Y, 20d) != 0)
                    newStart.Y = ComponentHelper.Snap(newStart, this.GridSize).Y;
                if (Math.IEEERemainder(newEnd.X, 20d) != 0)
                    newEnd.X = ComponentHelper.Snap(newEnd, this.GridSize).X;
                if (Math.IEEERemainder(newEnd.Y, 20d) != 0)
                    newEnd.Y = ComponentHelper.Snap(newEnd, this.GridSize).Y;
            }
            if (this.SnapToHV) // snap to horizontal or vertical
            {
                double height = Math.Max(newStart.Y, newEnd.Y) - Math.Min(newStart.Y, newEnd.Y);
                double length = Math.Sqrt(Math.Pow(newEnd.X - newStart.X, 2d) + Math.Pow(newEnd.Y - newStart.Y, 2d));
                double bearing = Math.Acos(height / length) * (180 / Math.PI);

                if (bearing <= 45 && switched)
                    newStart.X = newEnd.X;
                else if (bearing <= 45 && !switched)
                    newEnd.X = newStart.X;
                else if (bearing > 45 && switched)
                    newStart.Y = newEnd.Y;
                else
                    newEnd.Y = newStart.Y;
            }

            if (newStart.X > newEnd.X || newStart.Y > newEnd.Y)
            {
                component.StartLocation = newEnd;
                component.EndLocation = newStart;
            }
            else
            {
                component.StartLocation = newStart;
                component.EndLocation = newEnd;
            }

            component.ImplementMinimumSize(this.GridSize);

            component.UpdateLayout(); // custom update
        }

        public void Render(IRenderer dc, DrawingContext rc = null)
        {
            foreach (EComponent component in m_components)
            {
                if (component == SelectedComponent)
                    component.Render(dc, Colors.DarkOrange);
                else
                    component.Render(dc, Colors.Black);
            }
            foreach (EComponent component in m_tempComponents)
            {
                if (component == SelectedComponent)
                    component.Render(dc, Colors.DarkOrange);
                else
                    component.Render(dc, Colors.Black);
            }


            // joins
            foreach (Point join in m_joins)
            {
                dc.DrawEllipse(Colors.Black, Colors.Black, 1d, join, 3d, 3d);
            }
            // selection outlines
            if (rc == null)
                return;
            foreach (EComponent component in m_components)
            {
                Pen dashPen = new Pen(Brushes.Gray, 1.0f);
                dashPen.DashStyle = new DashStyle(new double[] { 4, 4 }, 0);
                if (component == SelectedComponent)
                {
                    rc.DrawRectangle(null, dashPen, component.BoundingBox);
                    if (!MainWindow.m_moveComponent || !SelectedComponent.CanResize)
                        return;
                    if (SelectedComponent.Horizontal)
                    {
                        rc.DrawRectangle(Brushes.Gray, null, new Rect(SelectedComponent.BoundingBox.X - 3, SelectedComponent.BoundingBox.Y +
                        SelectedComponent.BoundingBox.Height / 2 - 3f, 6, 6));
                        rc.DrawRectangle(Brushes.Gray, null, new Rect(SelectedComponent.BoundingBox.Right - 3, SelectedComponent.BoundingBox.Y + SelectedComponent.BoundingBox.Height / 2 - 3f, 6f, 6f));
                    }
                    else
                    {
                        rc.DrawRectangle(Brushes.Gray, null, new Rect(SelectedComponent.BoundingBox.X + SelectedComponent.BoundingBox.Width / 2 - 3f,
                            SelectedComponent.BoundingBox.Y - 3f, 6f, 6f));
                        rc.DrawRectangle(Brushes.Gray, null, new Rect(SelectedComponent.BoundingBox.X + SelectedComponent.BoundingBox.Width / 2 - 3f,
                            SelectedComponent.BoundingBox.Y + SelectedComponent.BoundingBox.Height - 3f, 6f, 6f));
                    }
                }
            }
        }

        private List<Point> m_joins = new List<Point>();
        private void UpdateJoins()
        {
            m_joins.Clear();
            foreach (EComponent component1 in m_components)
            {
                foreach (EComponent component2 in m_components)
                {
                    if (component1 == component2)
                        continue;
                    if (component1.Horizontal)
                    {
                        if (!component2.Horizontal)
                        {
                            Point intersection = new Point();
                            // check if other's end in this middle
                            if (AlmostEquals(component2.StartLocation.Y, component1.StartLocation.Y) && (component2.StartLocation.X + 1) < component1.EndLocation.X && component2.StartLocation.X > (component1.StartLocation.X + 1))
                            {
                                intersection = component2.StartLocation;
                            }
                            if (AlmostEquals(component2.EndLocation.Y, component1.StartLocation.Y) && (component2.EndLocation.X + 1) < component1.EndLocation.X && component2.EndLocation.X > (component1.StartLocation.X + 1))
                            {
                                intersection = component2.EndLocation;
                            }
                            // check if this end in other's middle
                            if (AlmostEquals(component1.StartLocation.X, component2.StartLocation.X) && (component1.StartLocation.Y + 1) < component2.EndLocation.Y && component1.StartLocation.Y > (component2.StartLocation.Y + 1))
                            {
                                intersection = component1.StartLocation;
                            }
                            if (AlmostEquals(component1.EndLocation.X, component2.StartLocation.X) && (component1.EndLocation.Y + 1) < component2.EndLocation.Y && component1.EndLocation.Y > (component2.StartLocation.Y + 1))
                            {
                                intersection = component1.EndLocation;
                            }

                            if (intersection != new Point())
                            {
                                if (!m_joins.Contains(intersection))
                                    m_joins.Add(intersection);
                            }
                        }
                    }
                }
            }
        }

        public void InvalidateVisual()
        {
            UpdateJoins();
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
            writer.WriteAttributeString("cd-version", winAbout.AppVersion);
            writer.WriteAttributeString("width", Size.Width.ToString());
            writer.WriteAttributeString("height", Size.Height.ToString());
            foreach (EComponent component in m_components)
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

        public void Save(string path, double displayWidth, double displayHeight)
        {
            XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("circuit");
            writer.WriteAttributeString("version", "1.1");
            writer.WriteAttributeString("cd-version", winAbout.AppVersion);
            writer.WriteAttributeString("width", displayWidth.ToString());
            writer.WriteAttributeString("height", displayHeight.ToString());
            foreach (EComponent component in m_components)
            {
                writer.WriteStartElement("component");
                Dictionary<string, object> properties = new Dictionary<string, object>();
                component.Serialize(properties);
                if (!properties.ContainsKey("type"))
                    properties.Add("type", component.GetType().Name.ToString());
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
            m_tempComponents.Clear();
            m_components.Clear();
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
                        EComponent component = (EComponent)Activator.CreateInstance(Type.GetType("CircuitDiagram.EComponents." + componentType, true, false));
                        Dictionary<string, object> properties = new Dictionary<string, object>();
                        reader.MoveToNextAttribute();
                        for (int i = 0; i < reader.AttributeCount; i++)
                        {
                            properties.Add(reader.Name, reader.Value);
                            reader.MoveToNextAttribute();
                        }
                        reader.MoveToElement();
                        component.Deserialize(properties);
                        UpdateComponent(component);
                        component.Editor.Document = this;

                        m_components.Add(component);
                    }
                    catch (Exception)
                    {
                        errorOccurred = true;
                    }
                }
            }
            reader.Close();
            if (errorOccurred)
                return DocumentLoadResult.FailIncorrectFormat;
            else
                return DocumentLoadResult.Success;
        }

        public void Load(string path, out double displayWidth, out double displayHeight)
        {
            bool errorOccurred = false;
            try
            {
                XmlTextReader reader = new XmlTextReader(path);
                m_tempComponents.Clear();
                m_components.Clear();
                displayWidth = 640;
                displayHeight = 480;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Depth == 0)
                    {
                        try
                        {
                            reader.MoveToAttribute("width");
                            displayWidth = double.Parse(reader.Value);
                            reader.MoveToAttribute("height");
                            displayHeight = double.Parse(reader.Value);
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
                            EComponent component = (EComponent)Activator.CreateInstance(Type.GetType("CircuitDiagram.EComponents." + componentType, true, false));
                            Dictionary<string, object> properties = new Dictionary<string, object>();
                            reader.MoveToNextAttribute();
                            for (int i = 0; i < reader.AttributeCount; i++)
                            {
                                properties.Add(reader.Name, reader.Value);
                                reader.MoveToNextAttribute();
                            }
                            reader.MoveToElement();
                            component.Deserialize(properties);
                            UpdateComponent(component);
                            component.Editor.Document = this;

                            m_components.Add(component);
                        }
                        catch (Exception)
                        {
                            errorOccurred = true;
                        }
                    }
                }
                reader.Close();
            }
            catch (Exception)
            {
                displayWidth = 640;
                displayHeight = 480;
                errorOccurred = true;
            }
            if (errorOccurred)
                MessageBox.Show("The document was not in the correct format.", "Error Loading Document", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion
    }
}
