// Condition.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2011-2014 Sam Fisher
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
using System.Text.RegularExpressions;

namespace CircuitDiagram.Components.Description
{
    public class Condition : IConditionTreeItem
    {
        private static Condition emptyCondition = new Condition();
        public static Condition Empty { get { return emptyCondition; } }

        public ConditionType Type { get; private set; }
        public ConditionComparison Comparison { get; private set; }
        public string VariableName { get; private set; }
        public object CompareTo { get; private set; }

        private Condition()
        {
            Type = ConditionType.Empty;
        }

        public Condition(ConditionType type, string name, ConditionComparison comparison, object compareTo)
        {
            Type = type;
            VariableName = name;
            Comparison = comparison;
            CompareTo = compareTo;
        }

        public static Condition ParseV1_1(string value)
        {
            ConditionType type;
            if (value.IndexOf("_") <= 1 && value.IndexOf("_") != -1)
                type = ConditionType.State;
            else
                type = ConditionType.Property;

            ConditionComparison comparisonType = ConditionComparison.Equal;
            object compareTo = true;

            Regex opCheck = new Regex("(==|>|<|<=|>=|!=)");
            Match opMatch = opCheck.Match(value);
            if (opMatch.Success)
            {
                int compareToIndex = opMatch.Index + opMatch.Length;
                string compareToStr = value.Substring(compareToIndex);

                switch (opMatch.Value)
                {
                    case ">":
                        comparisonType = ConditionComparison.Greater;
                        compareTo = double.Parse(compareToStr);
                        break;
                    case ">=":
                        comparisonType = ConditionComparison.GreaterOrEqual;
                        compareTo = double.Parse(compareToStr);
                        break;
                    case "<":
                        comparisonType = ConditionComparison.Less;
                        compareTo = double.Parse(compareToStr);
                        break;
                    case "<=":
                        comparisonType = ConditionComparison.LessOrEqual;
                        compareTo = double.Parse(compareToStr);
                        break;
                    case "!=":
                        comparisonType = ConditionComparison.NotEqual;
                        compareTo = compareToStr;
                        break;
                    case "==":
                        comparisonType = ConditionComparison.Equal;
                        compareTo = compareToStr;
                        break;
                }
            } // Else implicit '==true'

            if (value.StartsWith("!"))
            {
                if (comparisonType == ConditionComparison.Equal)
                    comparisonType = ConditionComparison.NotEqual;
                else if (comparisonType == ConditionComparison.NotEqual)
                    comparisonType = ConditionComparison.Equal;
                else if (comparisonType == ConditionComparison.Empty)
                    comparisonType = ConditionComparison.NotEmpty;
            }

            string variableName = Regex.Match(value, "\\$[a-zA-Z0-9]+").Value.Replace("$", "");
            if (type == ConditionType.State)
                variableName = value.Replace("_", "").Replace("!", "");

            return new Condition(type, variableName, comparisonType, compareTo);
        }

        public static Condition Parse(string value)
        {
            ConditionType type;
            if (value.IndexOf("_") <= 1 && value.IndexOf("_") != -1)
                type = ConditionType.State;
            else
                type = ConditionType.Property;

            ConditionComparison comparisonType = ConditionComparison.Equal;
            Regex ltCheck = new Regex("\\(lt_[0-9.]+\\)");
            Match ltMatch = ltCheck.Match(value);
            Regex gtCheck = new Regex("\\(gt_[0-9.]+\\)");
            Match gtMatch = gtCheck.Match(value);
            Regex eqCheck = new Regex("\\(eq_[a-zA-Z0-9.]+\\)");
            Match eqMatch = eqCheck.Match(value);
            Regex lteqCheck = new Regex("\\(lteq_[0-9.]+\\)");
            Match lteqMatch = lteqCheck.Match(value);
            Regex gteqCheck = new Regex("\\(gteq_[0-9.]+\\)");
            Match gteqMatch = gteqCheck.Match(value);
            Regex emptyCheck = new Regex("\\(empty\\)");
            Match emptyMatch = emptyCheck.Match(value);

            object compareTo = true;
            if (ltMatch.Success)
            {
                comparisonType = ConditionComparison.Less;
                compareTo = double.Parse(ltMatch.Value.Replace("(lt_", "").Replace(")", ""));
            }
            else if (gtMatch.Success)
            {
                comparisonType = ConditionComparison.Greater;
                compareTo = double.Parse(gtMatch.Value.Replace("(gt_", "").Replace(")", ""));
            }
            else if (eqMatch.Success)
            {
                compareTo = eqMatch.Value.Replace("(eq_", "").Replace(")", "");
            }
            else if (lteqMatch.Success)
            {
                comparisonType = ConditionComparison.LessOrEqual;
                compareTo = double.Parse(lteqMatch.Value.Replace("(lteq_", "").Replace(")", ""));
            }
            else if (gteqMatch.Success)
            {
                comparisonType = ConditionComparison.GreaterOrEqual;
                compareTo = double.Parse(gteqMatch.Value.Replace("(gteq_", "").Replace(")", ""));
            }
            else if (emptyMatch.Success)
            {
                comparisonType = ConditionComparison.Empty;
                compareTo = "";
            }

            if (value.StartsWith("!"))
            {
                if (comparisonType == ConditionComparison.Equal)
                    comparisonType = ConditionComparison.NotEqual;
                else if (comparisonType == ConditionComparison.NotEqual)
                    comparisonType = ConditionComparison.Equal;
                else if (comparisonType == ConditionComparison.Empty)
                    comparisonType = ConditionComparison.NotEmpty;
            }

            string variableName = Regex.Match(value, "\\$[a-zA-Z]+").Value.Replace("$", "").Replace("!", "");
            if (type == ConditionType.State)
                variableName = value.Replace("_", "").Replace("!", "");

            return new Condition(type, variableName, comparisonType, compareTo);
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

        public bool ConditionsAreMet(Component component)
        {
            return this.IsMet(component);
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
            return (this.Type == ConditionType.Property ? "$" : "_") +
                this.VariableName + 
                ComparisonToString(this.Comparison) + this.CompareTo.ToString();
        }
    }

    public enum ConditionComparison
    {
        Equal = 0,
        NotEqual = 1,
        Less = 2,
        LessOrEqual = 3,
        Greater = 4,
        GreaterOrEqual = 5,
        Empty = 6,
        NotEmpty = 7
    }

    public enum ConditionType : ushort
    {
        Empty = 3,
        Property = 0,
        State = 1
    }
}
