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

using CircuitDiagram.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Parsers.Conditions
{
    /// <summary>
    /// Parses conditions in the form "$prop(eq_a),$prop2(eq_b)".
    /// </summary>
    public class LegacyConditionParser : IConditionParser
    {
        private readonly ComponentDescription description;

        public LegacyConditionParser(ComponentDescription description)
        {
            this.description = description;
        }

        public IConditionTreeItem Parse(ComponentDescription description, string input)
        {
            var andList = new Stack<ConditionTreeLeaf>();

            string[] conditions = input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string condition in conditions)
                andList.Push(ParseLeaf(condition));

            return LegacyConditionParser.AndListToTree(andList);
        }

        public static IConditionTreeItem AndListToTree(Stack<ConditionTreeLeaf> andList)
        {
            if (andList.Count > 1)
            {
                ConditionTree previous = new ConditionTree(ConditionTree.ConditionOperator.AND, andList.Pop(), ConditionTree.Empty);
                while (andList.Count > 0)
                    previous = new ConditionTree(ConditionTree.ConditionOperator.AND, previous, andList.Pop());
                return previous;
            }
            else if (andList.Count == 1)
                return andList.Pop();
            else
                return ConditionTree.Empty;
        }

        private ConditionTreeLeaf ParseLeaf(string value)
        {
            ConditionType type;
            if (value.IndexOf("_") <= 1 && value.IndexOf("_") != -1)
                type = ConditionType.State;
            else
                type = ConditionType.Property;

            ConditionComparison comparisonType = ConditionComparison.Truthy;
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

            string compareTo = "true";
            if (ltMatch.Success)
            {
                comparisonType = ConditionComparison.Less;
                compareTo = ltMatch.Value.Replace("(lt_", "").Replace(")", "");
            }
            else if (gtMatch.Success)
            {
                comparisonType = ConditionComparison.Greater;
                compareTo = gtMatch.Value.Replace("(gt_", "").Replace(")", "");
            }
            else if (eqMatch.Success)
            {
                comparisonType = ConditionComparison.Equal;
                compareTo = eqMatch.Value.Replace("(eq_", "").Replace(")", "");
            }
            else if (lteqMatch.Success)
            {
                comparisonType = ConditionComparison.LessOrEqual;
                compareTo = lteqMatch.Value.Replace("(lteq_", "").Replace(")", "");
            }
            else if (gteqMatch.Success)
            {
                comparisonType = ConditionComparison.GreaterOrEqual;
                compareTo = gteqMatch.Value.Replace("(gteq_", "").Replace(")", "");
            }
            else if (emptyMatch.Success)
            {
                comparisonType = ConditionComparison.Falsy;
                compareTo = "";
            }

            if (value.StartsWith("!"))
            {
                if (comparisonType == ConditionComparison.Equal)
                    comparisonType = ConditionComparison.NotEqual;
                else if (comparisonType == ConditionComparison.NotEqual)
                    comparisonType = ConditionComparison.Equal;
                else if (comparisonType == ConditionComparison.Falsy)
                    comparisonType = ConditionComparison.Truthy;
                else if (comparisonType == ConditionComparison.Truthy)
                    comparisonType = ConditionComparison.Falsy;
            }
            
            string variableName = Regex.Match(value, "\\$[a-zA-Z]+").Value.Replace("$", "").Replace("!", "");
            ComponentProperty property;
            if (type == ConditionType.State)
            {
                variableName = value.Replace("_", "").Replace("!", "").ToLowerInvariant();
                return new ConditionTreeLeaf(type, variableName, comparisonType, PropertyValue.Parse(compareTo, PropertyValue.Type.Boolean));
            }
            else if ((property = description.Properties.FirstOrDefault(x => x.Name == variableName)) != null)
            {
                return new ConditionTreeLeaf(type, variableName, comparisonType, PropertyValue.Parse(compareTo, ConditionParser.ToSimplePropertyType(property.Type)));
            }
            else
            {
                throw new ConditionFormatException(string.Format("Unknown property '{0}'", variableName), 0, 0);
            }
        }
    }
}
