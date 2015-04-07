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
using System.Text.RegularExpressions;

namespace CircuitDiagram.Components.Conditions
{
    public class ConditionTreeLeaf : IConditionTreeItem
    {
        public ConditionType Type { get; private set; }
        public ConditionComparison Comparison { get; private set; }
        public string VariableName { get; private set; }
        public object CompareTo { get; private set; }

        internal ConditionTreeLeaf()
        {
            Type = ConditionType.Empty;
        }

        public ConditionTreeLeaf(ConditionType type, string name, ConditionComparison comparison, object compareTo)
        {
            Type = type;
            VariableName = name;
            Comparison = comparison;
            CompareTo = compareTo;
        }

        public bool IsMet(Component component)
        {
            if (Type == ConditionType.Empty)
            {
                return true;
            }
            else if (Type == ConditionType.State)
            {
                if (VariableName.ToLower() == "horizontal")
                {
                    if (Comparison == ConditionComparison.Equal)
                        return (component.Orientation == Orientation.Horizontal) == (bool)CompareTo;
                    else
                        return (component.Orientation == Orientation.Horizontal) != (bool)CompareTo;
                }
            }
            else
            {
                if (Comparison == ConditionComparison.Equal && CompareTo.GetType() == typeof(string))
                    return String.Equals(component.GetProperty(component.FindProperty(VariableName)) as string, CompareTo as string, StringComparison.InvariantCultureIgnoreCase);
                else if (Comparison == ConditionComparison.Equal && CompareTo.GetType() == typeof(bool))
                    return bool.Equals((bool)component.GetProperty(component.FindProperty(VariableName)), (bool)CompareTo);
                else if (Comparison == ConditionComparison.Equal)
                    return component.GetProperty(component.FindProperty(VariableName)) == CompareTo;
                else if (Comparison == ConditionComparison.NotEqual && CompareTo.GetType() == typeof(string))
                    return !String.Equals(component.GetProperty(component.FindProperty(VariableName)) as string, CompareTo as string, StringComparison.InvariantCultureIgnoreCase);
                else if (Comparison == ConditionComparison.NotEqual && CompareTo.GetType() == typeof(bool))
                    return !bool.Equals((bool)component.GetProperty(component.FindProperty(VariableName)), (bool)CompareTo);
                else if (Comparison == ConditionComparison.NotEqual)
                    return component.GetProperty(component.FindProperty(VariableName)) != CompareTo;
                else if (Comparison == ConditionComparison.Less)
                {
                    object propertyValue = component.GetProperty(component.FindProperty(VariableName));
                    if (propertyValue.GetType() == typeof(double))
                        return (double)propertyValue < (double)CompareTo;
                    else if (propertyValue.GetType() == typeof(int))
                        return (int)propertyValue < (int)CompareTo;
                }
                else if (Comparison == ConditionComparison.Greater)
                {
                    object propertyValue = component.GetProperty(component.FindProperty(VariableName));
                    if (propertyValue.GetType() == typeof(double))
                        return (double)propertyValue > (double)CompareTo;
                    else if (propertyValue.GetType() == typeof(int))
                        return (int)propertyValue > (int)CompareTo;
                }
                else if (Comparison == ConditionComparison.GreaterOrEqual)
                {
                    object propertyValue = component.GetProperty(component.FindProperty(VariableName));
                    if (propertyValue.GetType() == typeof(double))
                        return (double)propertyValue >= (double)CompareTo;
                    else if (propertyValue.GetType() == typeof(int))
                        return (int)propertyValue >= (int)CompareTo;
                }
                else if (Comparison == ConditionComparison.LessOrEqual)
                {
                    object propertyValue = component.GetProperty(component.FindProperty(VariableName));
                    if (propertyValue.GetType() == typeof(double))
                        return (double)propertyValue <= (double)CompareTo;
                    else if (propertyValue.GetType() == typeof(int))
                        return (int)propertyValue <= (int)CompareTo;
                }
                else if (Comparison == ConditionComparison.Empty)
                {
                    object propertyValue = component.GetProperty(component.FindProperty(VariableName));
                    return String.IsNullOrEmpty(propertyValue.ToString());
                }
                else if (Comparison == ConditionComparison.NotEmpty)
                {
                    object propertyValue = component.GetProperty(component.FindProperty(VariableName));
                    return !String.IsNullOrEmpty(propertyValue.ToString());
                }
            }

            return false;
        }

        public bool Compare(object value)
        {
                if (Comparison == ConditionComparison.Equal && CompareTo.GetType() == typeof(string))
                    return String.Equals(value as string, CompareTo as string, StringComparison.InvariantCultureIgnoreCase);
                else if (Comparison == ConditionComparison.Equal && CompareTo.GetType() == typeof(bool))
                    return bool.Equals(value, (bool)CompareTo);
                else if (Comparison == ConditionComparison.Equal)
                    return value == CompareTo;
                else if (Comparison == ConditionComparison.NotEqual && CompareTo.GetType() == typeof(string))
                    return !String.Equals(value as string, CompareTo as string, StringComparison.InvariantCultureIgnoreCase);
                else if (Comparison == ConditionComparison.NotEqual && CompareTo.GetType() == typeof(bool))
                    return !bool.Equals(value, (bool)CompareTo);
                else if (Comparison == ConditionComparison.NotEqual)
                    return value != CompareTo;
                else if (Comparison == ConditionComparison.Less)
                {
                    object propertyValue = value;
                    if (propertyValue.GetType() == typeof(double))
                        return (double)propertyValue < (double)CompareTo;
                    else if (propertyValue.GetType() == typeof(int))
                        return (int)propertyValue < (int)CompareTo;
                }
                else if (Comparison == ConditionComparison.Greater)
                {
                    object propertyValue = value;
                    if (propertyValue.GetType() == typeof(double))
                        return (double)propertyValue > (double)CompareTo;
                    else if (propertyValue.GetType() == typeof(int))
                        return (int)propertyValue > (int)CompareTo;
                }
                else if (Comparison == ConditionComparison.GreaterOrEqual)
                {
                    object propertyValue = value;
                    if (propertyValue.GetType() == typeof(double))
                        return (double)propertyValue >= (double)CompareTo;
                    else if (propertyValue.GetType() == typeof(int))
                        return (int)propertyValue >= (int)CompareTo;
                }
                else if (Comparison == ConditionComparison.LessOrEqual)
                {
                    object propertyValue = value;
                    if (propertyValue.GetType() == typeof(double))
                        return (double)propertyValue <= (double)CompareTo;
                    else if (propertyValue.GetType() == typeof(int))
                        return (int)propertyValue <= (int)CompareTo;
                }
                else if (Comparison == ConditionComparison.Empty)
                {
                    return String.IsNullOrEmpty(value.ToString());
                }
                else if (Comparison == ConditionComparison.NotEmpty)
                {
                    return !String.IsNullOrEmpty(value.ToString());
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
