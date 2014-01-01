// ComponentHelper.cs
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
using System.Xml;
using System.IO;
using CircuitDiagram.Components.Description;

namespace CircuitDiagram.Components
{
    public static class ComponentHelper
    {
        public static CreateComponentEditorDelegate CreateEditor;

        private static List<ComponentDescription> m_descriptions = new List<ComponentDescription>();

        public static IEnumerable<ComponentDescription> ComponentDescriptions { get { return m_descriptions.AsEnumerable(); } }

        private static Dictionary<string, Dictionary<string, ComponentIdentifier>> m_standardComponents = new Dictionary<string, Dictionary<string, ComponentIdentifier>>();

        public static double GridSize { get { return 10d; } }

        public static double SmallTextSize { get { return 10d; } }

        public static double LargeTextSize { get { return 12d; } }

        public static ComponentDescription WireDescription { get; set; }

        public static void ImplementMinimumSize(this Component component, double size)
        {
            if (component.Size < size)
                component.Size = size;
        }

        public static void SizeComponent(Component component, Point start, Point end)
        {
            // reverse points if necessary
            Point newStart = start;
            Point newEnd = end;
            bool switched = false;
            if (start.X < end.X)
            {
                newStart = end;
                newEnd = start;
                switched = true;
            }

            if (true) // snap to grid
            {
                if (Math.IEEERemainder(newStart.X, 20d) != 0)
                    newStart.X = ComponentHelper.Snap(newStart, GridSize).X;
                if (Math.IEEERemainder(newStart.Y, 20d) != 0)
                    newStart.Y = ComponentHelper.Snap(newStart, GridSize).Y;
                if (Math.IEEERemainder(newEnd.X, 20d) != 0)
                    newEnd.X = ComponentHelper.Snap(newEnd, GridSize).X;
                if (Math.IEEERemainder(newEnd.Y, 20d) != 0)
                    newEnd.Y = ComponentHelper.Snap(newEnd, GridSize).Y;
            }
            if (true) // snap to horizontal or vertical
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
                component.Location = new Vector(newEnd.X, newEnd.Y);
                if (newStart.X == newEnd.X)
                {
                    component.Size = newStart.Y - newEnd.Y;
                    component.Orientation = Orientation.Vertical;
                }
                else
                {
                    component.Size = newStart.X - newEnd.X;
                    component.Orientation = Orientation.Horizontal;
                }
            }
            else
            {
                component.Location = new Vector(newStart.X, newStart.Y);
                if (newStart.X == newEnd.X)
                {
                    component.Size = newEnd.Y - newStart.Y;
                    component.Orientation = Orientation.Vertical;
                }
                else
                {
                    component.Size = newEnd.X - newStart.X;
                    component.Orientation = Orientation.Horizontal;
                }
            }

            FlagOptions flagOptions = ApplyFlags(component);
            if ((flagOptions & FlagOptions.HorizontalOnly) == FlagOptions.HorizontalOnly && component.Orientation == Orientation.Vertical)
            {
                component.Orientation = Orientation.Horizontal;
                component.Size = component.Description.MinSize;
            }
            else if ((flagOptions & FlagOptions.VerticalOnly) == FlagOptions.VerticalOnly && component.Orientation == Orientation.Horizontal)
            {
                component.Orientation = Orientation.Vertical;
                component.Size = component.Description.MinSize;
            }

            component.ImplementMinimumSize(GridSize);
            component.ImplementMinimumSize(component.Description.MinSize);

            component.ResetConnections();
        }

        public static Point Snap(Point point, double gridSize)
        {
            return new Point(Math.Round(point.X / gridSize) * gridSize, Math.Round(point.Y / gridSize) * gridSize);
        }

        public static string ConvertToSentenceCase(string text)
        {
            return text;
        }

        private static int m_runtimeIDCounter = 0;
        private static int NewRuntimeID()
        {
            m_runtimeIDCounter++;
            return m_runtimeIDCounter;
        }

        public static void AddDescription(ComponentDescription description)
        {
            if (description.RuntimeID == 0)
                description.RuntimeID = NewRuntimeID();

            if (!m_descriptions.Contains(description))
                m_descriptions.Add(description);
        }

        public static ComponentDescription FindDescription(string name)
        {
            return m_descriptions.FirstOrDefault(description => description.ComponentName == name);
        }

        public static ComponentDescription FindDescription(Guid guid)
        {
            return m_descriptions.FirstOrDefault(description => description.Metadata.GUID == guid);
        }

        public static ComponentDescription FindDescriptionByRuntimeID(int runtimeID)
        {
            return m_descriptions.FirstOrDefault(description => description.RuntimeID == runtimeID);
        }

        public static FlagOptions ApplyFlags(Component component)
        {
            FlagOptions returnOptions = FlagOptions.None;

            foreach (Conditional<FlagOptions> option in component.Description.Flags)
            {
                if (option.Conditions.ConditionsAreMet(component))
                    returnOptions |= option.Value;
            }

            return returnOptions;
        }

        public static bool IsStandardComponent(ComponentDescription componentDescription)
        {
            if (componentDescription.Metadata.GUID == Guid.Empty || componentDescription.Source == null)
                return false;
#if DEBUG
                return (System.IO.Path.GetDirectoryName(componentDescription.Source.Path) == System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ext"
                    || System.IO.Path.GetDirectoryName(componentDescription.Source.Path) == Path.GetFullPath("../../Components"));
#else
                return (System.IO.Path.GetDirectoryName(componentDescription.Source.Path) == System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ext");
#endif
        }

        public static ComponentProperty FindProperty(ComponentDescription description, string key, PropertySearchKey keyType)
        {
            foreach (ComponentProperty property in description.Properties)
            {
                if (keyType == PropertySearchKey.FullName && property.Name == key)
                    return property;
                else if (keyType == PropertySearchKey.SerializedName && property.SerializedName == key)
                    return property;
            }
            return null;
        }

        internal static bool ShouldEmbedDescription(ComponentDescription componentDescription)
        {
            if (EmbedOptions == ComponentEmbedOptions.All)
                return true;
            else if (EmbedOptions == ComponentEmbedOptions.None)
                return false;
            else
                return !IsStandardComponent(componentDescription);
        }

        public static string[] InstalledComponentDirectories
        {
            get
            {
                return new string[]
                {
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ext"
                };
            }
        }

        public static string SerializeToString(this Component component)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            component.Serialize(properties);
            return ComponentDataString.ConvertToString(properties);
        }

        public static LoadIconDelegate LoadIcon;

        public static ComponentUpdatedDelegate ComponentUpdatedDelegate { get; set; }

        public static void SetStandardComponent(string collection, string item, ComponentDescription description, ComponentConfiguration configuration)
        {
            if (!m_standardComponents.ContainsKey(collection))
                m_standardComponents.Add(collection, new Dictionary<string, ComponentIdentifier>());
            if (!m_standardComponents[collection].ContainsKey(item))
                m_standardComponents[collection].Add(item, new ComponentIdentifier(description, configuration));
            else
                m_standardComponents[collection][item] = new ComponentIdentifier(description, configuration);
        }

        public static ComponentIdentifier GetStandardComponent(string collection, string item)
        {
            if (collection == null || item == null)
                return null;

            if (m_standardComponents.ContainsKey(collection) && m_standardComponents[collection].ContainsKey(item))
                return m_standardComponents[collection][item];
            else
            {
                ComponentIdentifier identifier = null;
                foreach(ComponentDescription description in m_descriptions)
                {
                    if (description.Metadata.ImplementSet == collection && description.Metadata.ImplementItem == item)
                    {
                        identifier = new ComponentIdentifier(description);
                        break;
                    }
                    else if (description.Metadata.ImplementSet == collection)
                    {
                        foreach (ComponentConfiguration configuration in description.Metadata.Configurations)
                        {
                            if (configuration.ImplementationName == item)
                            {
                                identifier = new ComponentIdentifier(description, configuration);
                                break;
                            }
                        }
                    }
                    if (identifier != null)
                        break;
                }

                return identifier;
            }
        }

        /// <summary>
        /// Determines whether the specified ICircuitElement is a wire.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <returns>True if it is a wire, false otherwise.</returns>
        public static bool IsWire(CircuitDiagram.Elements.ICircuitElement element)
        {
            if (!(element is CircuitDiagram.Elements.IComponentElement))
                return false;

            CircuitDiagram.Elements.IComponentElement componentElement = element as CircuitDiagram.Elements.IComponentElement;

            return (componentElement.ImplementationCollection == CircuitDiagram.IO.ComponentCollections.Common && componentElement.ImplementationItem == "wire")
                || (componentElement is Component && (componentElement as Component).Description == WireDescription);
        }

        /// <summary>
        /// Determines whether the specified description is available.
        /// </summary>
        /// <param name="guid">The guid of the description to check for.</param>
        /// <returns>True if the description is available, false otherwise.</returns>
        public static bool IsDescriptionAvailable(Guid guid)
        {
            return ComponentDescriptions.FirstOrDefault(item => item.Metadata.GUID == guid) != null;
        }

        public static ComponentEmbedOptions EmbedOptions { get; set; }
    }

    public enum PropertySearchKey
    {
        FullName,
        SerializedName
    }

    public delegate object LoadIconDelegate(byte[] data, string mimeType);

    public enum ComponentEmbedOptions
    {
        All,
        Automatic,
        None
    }
}
