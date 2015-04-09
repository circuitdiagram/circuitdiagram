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
using CircuitDiagram.Components.Conditions;

namespace CircuitDiagram.Components.Description
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

        public string Format(Component component)
        {
            Regex variable = new Regex("\\$[a-zA-z]+ ");
            string plainVars = variable.Replace(Value, new MatchEvaluator(delegate(Match match)
            {
                string propertyName = match.Value.Replace("$", "").Trim().ToLowerInvariant();
                ComponentProperty propertyInfo = component.FindProperty(propertyName);
                if (propertyInfo != null)
                    return component.GetProperty(propertyInfo).ToString();
                return "";
            }));

            variable = new Regex("\\$[a-zA-Z]+[\\(\\)a-z_0-9]+ ");
            string formattedVars = variable.Replace(plainVars, new MatchEvaluator(delegate(Match match)
            {
                Regex propertyNameRegex = new Regex("\\$[a-zA-z]+");
                string propertyName = propertyNameRegex.Match(match.Value).Value.Replace("$", "").Trim().ToLowerInvariant();
                ComponentProperty propertyInfo = component.FindProperty(propertyName);
                if (propertyInfo != null)
                {
                    return ApplySpecialFormatting(component.GetProperty(propertyInfo), match.Value.Replace(propertyNameRegex.Match(match.Value).Value, "").Trim());
                }
                return "";
            }));

            Regex regex = new Regex(@"\\[uU]([0-9A-F]{4})");
            return regex.Replace(formattedVars, match => ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString());
        }

        private static string ApplySpecialFormatting(PropertyUnion property, string formatting)
        {
            string[] formatTasks = formatting.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string formatTask in formatTasks)
            {
                if (string.IsNullOrEmpty(formatTask))
                    continue;

                string[] parameters = formatTask.Split('_');
                string task = parameters[0];
                string option = parameters[1];

                switch (task)
                {
                    case "div":
                        if (property.InternalType == PropertyUnionType.Double)
                            property = new PropertyUnion((double)property.Value / double.Parse(option));
                        break;
                    case "mul":
                        if (property.InternalType == PropertyUnionType.Double)
                            property = new PropertyUnion((double)property.Value * double.Parse(option));
                        break;
                    case "round":
                        if (property.InternalType == PropertyUnionType.Double)
                            property = new PropertyUnion(Math.Round((double)property.Value, int.Parse(option)));
                        break;
                }
            }

            return property.ToString();
        }
    }
}
