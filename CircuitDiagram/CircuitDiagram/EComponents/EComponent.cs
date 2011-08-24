// EComponent.cs
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
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Reflection;
using CircuitDiagram.EComponents;
using System.Globalization;

namespace CircuitDiagram
{
    public abstract class EComponent
    {
        public static EComponent Create(string creationData)
        {
            creationData = creationData.Replace(",", "\r\n");
            EComponent newComponent;
            Dictionary<string, object> properties = ComponentStringDescription.ConvertToDictionary(creationData);
            newComponent = (EComponent)Activator.CreateInstance(Type.GetType("CircuitDiagram.EComponents." + properties["type"], true, true));
            newComponent.Deserialize(properties);
            return newComponent;
        }

        #region Properties
        public bool CanResize { get; protected set; }
        public bool CanFlip { get; protected set; }
        public bool IsFlipped { get; set; }

        private Point m_startLocation;
        private Point m_endLocation;

        public Point StartLocation
        {
            get { return m_startLocation; }
            set { m_startLocation = value; }
        }

        public Point EndLocation
        {
            get { return m_endLocation; }
            set { m_endLocation = value; }
        }

        protected Point RenderStartLocation
        {
            get
            {
                if (Horizontal)
                    return new Point(StartLocation.X - 1, StartLocation.Y);
                else
                    return new Point(StartLocation.X, StartLocation.Y - 1);
            }
        }

        protected Point RenderEndLocation
        {
            get
            {
                if (Horizontal)
                    return new Point(EndLocation.X + 1, EndLocation.Y);
                else
                    return new Point(EndLocation.X, EndLocation.Y + 1);
            }
        }

        public virtual Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(new Point(RenderStartLocation.X, RenderStartLocation.Y - 4), new Size(RenderEndLocation.X - RenderStartLocation.X, 8));
                else
                    return new Rect(new Point(RenderStartLocation.X - 4, RenderStartLocation.Y), new Size(8, RenderEndLocation.Y - RenderStartLocation.Y));
            }
        }

        public ComponentEditorBase Editor { get; set; }

        public bool Horizontal { get { return StartLocation.Y == EndLocation.Y; } }
        #endregion

        public EComponent()
        {
            IsFlipped = false;
            CanFlip = false;
            CanResize = true;
            Editor = new BasicComponentEditor(this);
            Serialize(new Dictionary<string, object>());
        }

        public virtual void UpdateLayout()
        {
        }

        public abstract void Render(IRenderer dc, Color color);

        /// <summary>
        /// Gets a list of all of the component's properties that can be serialized.
        /// </summary>
        /// <returns>List of ComponentPropertyInfo</returns>
        public List<ComponentPropertyInfo> SerializableProperties()
        {
            List<ComponentPropertyInfo> returnList = new List<ComponentPropertyInfo>();

            PropertyInfo[] classProperties = this.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in classProperties)
            {
                ComponentSerializableAttribute attribute = Attribute.GetCustomAttribute(propertyInfo, typeof(ComponentSerializableAttribute)) as ComponentSerializableAttribute;
                if (attribute != null)
                {
                    string serializeAs = attribute.SerializedName;
                    if (serializeAs == null && (attribute.Options & ComponentSerializeOptions.StoreLowercase) == ComponentSerializeOptions.StoreLowercase)
                        serializeAs = propertyInfo.Name.ToLower();
                    else if (serializeAs == null)
                        serializeAs = propertyInfo.Name;

                    string displayName = attribute.DisplayName;
                    if (displayName == null && (attribute.Options & ComponentSerializeOptions.DisplaySentenceCase) == ComponentSerializeOptions.DisplaySentenceCase)
                        displayName = ComponentHelper.ConvertToSentenceCase(propertyInfo.Name);
                    else if (displayName == null)
                        displayName = propertyInfo.Name;

                    bool alignLeft = (attribute.Options & ComponentSerializeOptions.DisplayAlignLeft) == ComponentSerializeOptions.DisplayAlignLeft;

                    returnList.Add(new ComponentPropertyInfo(serializeAs, displayName, alignLeft, propertyInfo));
                }
            }

            return returnList;
        }

        public void Serialize(Dictionary<string, object> properties)
        {
            // add common properties
            properties.Add("type", this.GetType().Name);
            properties.Add("x", StartLocation.X);
            properties.Add("y", StartLocation.Y);
            properties.Add("orientation", (Horizontal ? "horizontal" : "vertical"));
            if (CanResize)
            {
                if (Horizontal)
                    properties.Add("size", (EndLocation.X - StartLocation.X));
                else
                    properties.Add("size", (EndLocation.Y - StartLocation.Y));
            }
            if (CanFlip)
                properties.Add("flipped", IsFlipped);

            // add custom properties
            PropertyInfo[] classProperties = this.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in classProperties)
            {
                ComponentSerializableAttribute attribute = Attribute.GetCustomAttribute(propertyInfo, typeof(ComponentSerializableAttribute)) as ComponentSerializableAttribute;
                if (attribute != null)
                {
                    string serializeAs = attribute.SerializedName;
                    if (serializeAs == null && (attribute.Options & ComponentSerializeOptions.StoreLowercase) == ComponentSerializeOptions.StoreLowercase)
                        serializeAs = propertyInfo.Name.ToLower();
                    else if (serializeAs == null)
                        serializeAs = propertyInfo.Name;
                    if (propertyInfo.PropertyType.IsEnum)
                        properties.Add(serializeAs, (int)propertyInfo.GetValue(this, null));
                    else
                        properties.Add(serializeAs, propertyInfo.GetValue(this, null));
                }
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
            bool hasCheckedOrientation = false;
            bool horizontal = true;
            foreach (KeyValuePair<string, object> property in properties)
            {
                // load common properties
                if (property.Key == "x")
                    m_startLocation.X = (double)GetAsCorrectType(typeof(double), property.Value);
                if (property.Key == "y")
                    m_startLocation.Y = (double)GetAsCorrectType(typeof(double), property.Value);
                if (property.Key == "orientation" && property.Value.ToString().ToLower() == "vertical")
                {
                    horizontal = false;
                    hasCheckedOrientation = true;
                }
                if (property.Key == "size")
                {
                    if (!hasCheckedOrientation) // size property may be before orientation property
                    {
                        if (properties.ContainsKey("orientation") && properties["orientation"].ToString().ToLower() == "vertical")
                        {
                            horizontal = false;
                            hasCheckedOrientation = true;
                        }
                    }
                    double size = (double)GetAsCorrectType(typeof(double), property.Value);
                    if (horizontal)
                    {
                        m_endLocation.X = m_startLocation.X + size;
                        m_endLocation.Y = m_startLocation.Y;
                    }
                    else
                    {
                        m_endLocation.X = m_startLocation.X;
                        m_endLocation.Y = m_startLocation.Y + size;
                    }
                }
                IsFlipped = false;
                if (property.Key == "flipped" && property.Value.ToString().ToLower() == "true")
                    IsFlipped = true;

                // load custom properties
                MemberInfo[] memerInfo = this.GetType().FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, new MemberFilter(IsPropertySerializedMatch), property.Key);
                if (memerInfo.Length > 0)
                    (memerInfo[0] as PropertyInfo).SetValue(this, GetAsCorrectType((memerInfo[0] as PropertyInfo).PropertyType, property.Value), null);
            }

            if (!this.CanResize)
                EndLocation = StartLocation;
        }

        private bool IsPropertySerializedMatch(MemberInfo info, object filterCriteria)
        {
            // return false unless property has a ComponentSerializable attribute
            ComponentSerializableAttribute attribute = Attribute.GetCustomAttribute(info, typeof(ComponentSerializableAttribute)) as ComponentSerializableAttribute;
            if (attribute != null)
            {
                // check whether serialized name is explicitly defined
                if (attribute.SerializedName != null)
                    return attribute.SerializedName == filterCriteria as string; // check serialized name
                else if ((attribute.Options & ComponentSerializeOptions.StoreLowercase) == ComponentSerializeOptions.StoreLowercase)
                    return info.Name.ToLower() == filterCriteria as string; // check if lowercase match
                else
                    return info.Name == filterCriteria as string; // check property name
            }

            return false;
        }
    }
}
