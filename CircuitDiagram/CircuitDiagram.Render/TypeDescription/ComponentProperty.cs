#region Copyright & License Information
/*
 * Copyright 2012-2015 Sam Fisher
 *
 * This file is part of Circuit Diagram
 * http://www.circuit-diagram.org/
 * 
 * Circuit Diagram is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or (at
 * your option) any later version.
 */
#endregion

using CircuitDiagram.Components.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CircuitDiagram.Circuit;
using CircuitDiagram.Components;
using CircuitDiagram.TypeDescription.Conditions;

namespace CircuitDiagram.TypeDescription
{
    public class ComponentProperty
    {
        public string Name { get; private set; }
        public PropertyName SerializedName { get; private set; }
        public string DisplayName { get; private set; }
        public PropertyValue Default { get; private set; }
        public string[] EnumOptions { get; private set; }
        public PropertyType Type { get; private set; }
        public ComponentPropertyFormat[] FormatRules { get; private set; }
        public Dictionary<PropertyOtherConditionType, IConditionTreeItem> OtherConditions { get; private set; }

        public ComponentProperty(string name, PropertyName serializedName, string displayName, PropertyType type, PropertyValue defaultValue, ComponentPropertyFormat[] formatRules,
                                 Dictionary<PropertyOtherConditionType, IConditionTreeItem> otherConditions, string[] enumOptions = null)
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

        public string Format(PositionalComponent component, ComponentDescription description, PropertyValue value)
        {
            foreach (ComponentPropertyFormat formatRule in FormatRules)
            {
                if (formatRule.Conditions.IsMet(component, description))
                    return formatRule.Format(component, description);
            }
            return value.ToString();
        }
    }

    public enum PropertyType
    {
        Boolean,
        Decimal,
        Enum,
        Integer,
        String,
    }

    public static class PropertyTypeExtensions
    {
        public static PropertyValue.Type ToPropertyType(this PropertyType type)
        {
            switch (type)
            {
                case PropertyType.Boolean:
                    return PropertyValue.Type.Boolean;
                case PropertyType.Decimal:
                    return PropertyValue.Type.Numeric;
                case PropertyType.Enum:
                    return PropertyValue.Type.String;
                case PropertyType.Integer:
                    return PropertyValue.Type.Numeric;
                case PropertyType.String:
                    return PropertyValue.Type.String;
                default:
                    return PropertyValue.Type.Unknown;
            }
        }
    }
}
