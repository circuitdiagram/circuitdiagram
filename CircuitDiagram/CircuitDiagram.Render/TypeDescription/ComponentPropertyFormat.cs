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
using System.Globalization;
using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescription.Conditions;

namespace CircuitDiagram.TypeDescription
{
    public class ComponentPropertyFormat
    {
        public IConditionTreeItem Conditions { get; private set; }
        public string Value { get; private set; }

        public ComponentPropertyFormat(string value, IConditionTreeItem conditions)
        {
            Value = value;
            Conditions = conditions;
        }

        public string Format(Component component, ComponentDescription description)
        {
            Regex variable = new Regex("\\$[a-zA-z]+ ");
            string plainVars = variable.Replace(Value, new MatchEvaluator(delegate(Match match)
            {
                string propertyName = match.Value.Replace("$", "").Trim();
                var property = description.GetProperty(component, propertyName);
                return property.ToString();
            }));

            variable = new Regex("\\$[a-zA-Z]+[\\(\\)a-z_0-9]+ ");
            string formattedVars = variable.Replace(plainVars, new MatchEvaluator(delegate(Match match)
            {
                Regex propertyNameRegex = new Regex("\\$[a-zA-z]+");
                string propertyName = propertyNameRegex.Match(match.Value).Value.Replace("$", "").Trim();
                var property = description.GetProperty(component, propertyName);
                return ApplySpecialFormatting(property, match.Value.Replace(propertyNameRegex.Match(match.Value).Value, "").Trim());
            }));

            Regex regex = new Regex(@"\\[uU]([0-9A-F]{4})");
            return regex.Replace(formattedVars, match => ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString());
        }

        private static string ApplySpecialFormatting(PropertyValue property, string formatting)
        {
            string[] formatTasks = formatting.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string formatTask in formatTasks)
            {
                if (string.IsNullOrEmpty(formatTask))
                    continue;

                string[] parameters = formatTask.Split('_');
                string task = parameters[0];
                string option = parameters[1];

                if (!property.IsNumeric())
                    continue;

                switch (task)
                {
                    case "div":
                        property = new PropertyValue(property.NumericValue / double.Parse(option));
                        break;
                    case "mul":
                        property = new PropertyValue(property.NumericValue * double.Parse(option));
                        break;
                    case "round":
                        property = new PropertyValue(Math.Round(property.NumericValue, int.Parse(option)));
                        break;
                }
            }

            return property.ToString();
        }
    }
}
