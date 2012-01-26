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

namespace CircuitDiagram.Components
{
    public static class ComponentHelper
    {
        private static List<ComponentDescription> m_descriptions = new List<ComponentDescription>();
        private static List<string> m_standardComponentGUIDs;

        public static IEnumerable<ComponentDescription> ComponentDescriptions { get { return m_descriptions.AsEnumerable(); } }

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
                component.Offset = new Vector(newEnd.X, newEnd.Y);
                if (newStart.X == newEnd.X)
                {
                    component.Size = newStart.Y - newEnd.Y;
                    component.Horizontal = false;
                }
                else
                {
                    component.Size = newStart.X - newEnd.X;
                    component.Horizontal = true;
                }
            }
            else
            {
                component.Offset = new Vector(newStart.X, newStart.Y);
                if (newStart.X == newEnd.X)
                {
                    component.Size = newEnd.Y - newStart.Y;
                    component.Horizontal = false;
                }
                else
                {
                    component.Size = newEnd.X - newStart.X;
                    component.Horizontal = true;
                }
            }

            FlagOptions flagOptions = ApplyFlags(component);
            if ((flagOptions & FlagOptions.HorizontalOnly) == FlagOptions.HorizontalOnly && !component.Horizontal)
            {
                component.Horizontal = true;
                component.Size = component.Description.MinSize;
            }
            else if ((flagOptions & FlagOptions.VerticalOnly) == FlagOptions.VerticalOnly && component.Horizontal)
            {
                component.Horizontal = false;
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

        public static ComponentDescription FindDescription(string implementSet, string implementItem)
        {
            return m_descriptions.FirstOrDefault(description => description.Metadata.ImplementSet == implementSet && description.Metadata.ImplementItem == implementItem);
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
            if (componentDescription.Metadata.GUID == Guid.Empty)
                return false;
            if (m_standardComponentGUIDs == null)
            {
#if DEBUG
                return (System.IO.Path.GetDirectoryName(componentDescription.Source.Path) == System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ext"
                    || System.IO.Path.GetDirectoryName(componentDescription.Source.Path) == Path.GetFullPath("../../Components"));
#else
                return (System.IO.Path.GetDirectoryName(componentDescription.Source.Path) == System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ext");
#endif
            }
            return m_standardComponentGUIDs.Contains(componentDescription.Metadata.GUID.ToString());
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
            if (!IsStandardComponent(componentDescription))
                return true;
            return false;
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

        public static ComponentEditor.ComponentUpdatedDelegate ComponentUpdatedDelegate { get; set; }
    }

    public enum PropertySearchKey
    {
        FullName,
        SerializedName
    }
}
