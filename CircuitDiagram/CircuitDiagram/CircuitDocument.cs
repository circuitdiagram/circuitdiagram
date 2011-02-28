using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Xml;

namespace CircuitDiagram
{
    class CircuitDocument
    {
        const double GridSize = 10d;
        public event EventHandler VisualInvalidated;

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
        }

        public void Render(IRenderer dc)
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
        }

        public void UpdateLayout(EComponent component)
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

            if (SnapToGrid)
            {
                if (Math.IEEERemainder(newStart.X, 20d) != 0)
                    newStart.X = Snap(newStart).X;
                if (Math.IEEERemainder(newStart.Y, 20d) != 0)
                    newStart.Y = Snap(newStart).Y;
                if (Math.IEEERemainder(newEnd.X, 20d) != 0)
                    newEnd.X = Snap(newEnd).X;
                if (Math.IEEERemainder(newEnd.Y, 20d) != 0)
                    newEnd.Y = Snap(newEnd).Y;
            }
            if (SnapToHV)
            {
                double height = Biggest(newStart.Y, newEnd.Y) - Smallest(newStart.Y, newEnd.Y);
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

            if (component.Horizontal)
            {
                component.StartLocation = Point.Add(component.StartLocation, new Vector(-1d, 0d));
                component.EndLocation = Point.Add(component.EndLocation, new Vector(1d, 0d));
            }
            else
            {
                component.StartLocation = Point.Add(component.StartLocation, new Vector(0d, -1d));
                component.EndLocation = Point.Add(component.EndLocation, new Vector(0d, 1d));
            }
        }

        private static double Biggest(double one, double two)
        {
            return (one >= two ? one : two);
        }

        private static double Smallest(double one, double two)
        {
            return (one < two ? one : two);
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
                                if (!m_joins.Contains(Snap(intersection)))
                                    m_joins.Add(Snap(intersection));
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

        private static Point Snap(Point point)
        {
            return new Point(Math.Round(point.X / GridSize) * GridSize, Math.Round(point.Y / GridSize) * GridSize);
        }

        public void Save(string path, double displayWidth, double displayHeight)
        {
            XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("circuit");
            writer.WriteAttributeString("version", "1.0");
            writer.WriteAttributeString("width", displayWidth.ToString());
            writer.WriteAttributeString("height", displayHeight.ToString());
            foreach (EComponent component in m_components)
            {
                writer.WriteStartElement(component.GetType().Name);
                writer.WriteAttributeString("location", component.StartLocation.ToString() + "," + component.EndLocation.ToString());
                component.SaveData(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
        }

        public void Load(string path, out double displayWidth, out double displayHeight)
        {
            XmlTextReader reader = new XmlTextReader(path);
            m_tempComponents.Clear();
            m_components.Clear();
            bool errorOccurred = false;
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
                else if (reader.NodeType == XmlNodeType.Element && reader.Depth == 1)
                {
                    try
                    {
                        reader.MoveToAttribute("location");
                        string[] locationString = reader.Value.Split(',');
                        reader.MoveToElement();
                        Type.GetType("CircuitDiagram.EComponents." + reader.LocalName, true, false);
                        EComponent component = (EComponent)Activator.CreateInstance(Type.GetType("CircuitDiagram.EComponents." + reader.LocalName, true, false));
                        component.StartLocation = new Point(int.Parse(locationString[0]), int.Parse(locationString[1]));
                        component.EndLocation = new Point(int.Parse(locationString[2]), int.Parse(locationString[3]));
                        component.LoadData(reader);
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
                MessageBox.Show("The document was not in the correct format.", "Error Loading Document", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
