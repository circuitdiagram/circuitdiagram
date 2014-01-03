// ComponentProperty.cs
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

namespace CircuitDiagram.Components.Description
{
    public class ComponentProperty
    {
        public string Name { get; private set; }
        public string SerializedName { get; private set; }
        public string DisplayName { get; private set; }
        public object Default { get; private set; }
        public string[] EnumOptions { get; private set; }
        public Type Type { get; private set; }
        public ComponentPropertyFormat[] FormatRules { get; private set; }
        public Dictionary<PropertyOtherConditionType, IConditionTreeItem> OtherConditions { get; private set; }

        public ComponentProperty(string name, string serializedName, string displayName, Type type, object defaultValue, ComponentPropertyFormat[] formatRules, Dictionary<PropertyOtherConditionType, IConditionTreeItem> otherConditions, string[] enumOptions = null)
        {
            Name = name;
            SerializedName = serializedName;
            DisplayName = displayName;
            Type = type;
            Default = defaultValue;
            FormatRules = formatRules;
            EnumOptions = enumOptions;
            OtherConditions = otherConditions;
        }

        public string Format(Component component, object value)
        {
            foreach (ComponentPropertyFormat formatRule in FormatRules)
            {
                if (formatRule.Conditions.IsMet(component))
                    return formatRule.Format(component);
            }
            return value.ToString();
        }
    }
}
