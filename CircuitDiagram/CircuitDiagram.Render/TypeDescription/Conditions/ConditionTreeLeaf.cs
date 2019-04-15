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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CircuitDiagram.Circuit;
using CircuitDiagram.Components.Conditions;
using CircuitDiagram.Primitives;
using CircuitDiagram.TypeDescription.Extensions;

namespace CircuitDiagram.TypeDescription.Conditions
{
    public class ConditionTreeLeaf : IConditionTreeItem
    {
        public ConditionType Type { get; }
        public ConditionComparison Comparison { get; }
        public string VariableName { get; }
        public PropertyValue CompareTo { get; }

        internal ConditionTreeLeaf()
        {
            Type = ConditionType.Empty;
            Comparison = ConditionComparison.Falsy;
            VariableName = string.Empty;
            CompareTo = new PropertyValue();
        }

        public ConditionTreeLeaf(ConditionType type, string variableName, ConditionComparison comparison, PropertyValue compareTo)
        {
            Type = type;
            VariableName = variableName;
            Comparison = comparison;
            CompareTo = compareTo;
        }

        public bool IsMet(PositionalComponent component, ComponentDescription description)
        {
            if (Type == ConditionType.Empty)
                return true;
        
            if (Type == ConditionType.State)
            {
                if (VariableName.ToLower() == "horizontal")
                {
                    if (Comparison == ConditionComparison.Truthy || Comparison == ConditionComparison.Equal)
                        return (component.Layout.Orientation == Orientation.Horizontal) == CompareTo.BooleanValue;
                    
                    return (component.Layout.Orientation == Orientation.Horizontal) != CompareTo.BooleanValue;
                }
            }
            else
            {
                var propertyValue = description.GetProperty(component, VariableName);

                if (Comparison == ConditionComparison.Truthy)
                    return propertyValue.IsTruthy();

                if (Comparison == ConditionComparison.Falsy)
                    return !propertyValue.IsTruthy();

                int cv = propertyValue.CompareTo(CompareTo);
                switch (Comparison)
                {
                    case ConditionComparison.Equal:
                        return cv == 0;
                    case ConditionComparison.Greater:
                        return cv == 1;
                    case ConditionComparison.GreaterOrEqual:
                        return cv >= 0;
                    case ConditionComparison.Less:
                        return cv == -1;
                    case ConditionComparison.LessOrEqual:
                        return cv <= 0;
                    case ConditionComparison.NotEqual:
                        return cv != 0;
                }
            }

            return false;
        }

        private static string ComparisonToString(ConditionComparison c)
        {
            switch (c)
            {
                case ConditionComparison.Equal:
                    return "==";
                case ConditionComparison.Greater:
                    return ">";
                case ConditionComparison.GreaterOrEqual:
                    return ">=";
                case ConditionComparison.Less:
                    return "<";
                case ConditionComparison.LessOrEqual:
                    return "<=";
                case ConditionComparison.NotEqual:
                    return "!=";
                default:
                    return "?";
            }
        }

        public override string ToString()
        {
            return (this.Type == ConditionType.Property ? "$" : "") +
                this.VariableName + 
                ComparisonToString(this.Comparison) + this.CompareTo.ToString();
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }
            
            // If parameter cannot be cast to ConditionTreeLeaf return false.
            ConditionTreeLeaf o = obj as ConditionTreeLeaf;
            if ((System.Object)o == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            // Return true if the fields match:
            return (Type.Equals(o.Type)
                && Comparison.Equals(o.Comparison)
                && VariableName.Equals(o.VariableName)
                && CompareTo.Equals(o.CompareTo));
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode()
                ^ Comparison.GetHashCode()
                ^ VariableName.GetHashCode()
                ^ CompareTo.GetHashCode();
        }
    }
}
