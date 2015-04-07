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

using CircuitDiagram.Components.Conditions.Parsers;
using CircuitDiagram.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CircuitDiagram.Components.Conditions.Parsers
{
    /// <summary>
    /// Parses conditions in the form "$prop==a||$prop2==b".
    /// </summary>
    public class ConditionParser : IConditionParser
    {
        private readonly string[] legalStateNames = new string[]
        {
            "horizontal"
        };

        /// <summary>
        /// Gets or sets the format options to use when parsing.
        /// </summary>
        public ConditionFormat ParseFormat { get; set; }

        /// <summary>
        /// Creates a new ConditionParser using the default parse format.
        /// </summary>
        public ConditionParser()
        {
            ParseFormat = new ConditionFormat();
        }

        /// <summary>
        /// Creates a new ConditionParser using the specified parse format.
        /// </summary>
        public ConditionParser(ConditionFormat parseFormat)
        {
            ParseFormat = parseFormat;
        }

        public IConditionTreeItem Parse(string input)
        {
            // Tokenize

            var output = new Queue<ConditionToken>();
            var operators = new Stack<ConditionToken>();

            var reader = new PositioningReader(new StringReader(input));
            ConditionToken? token = ReadToken(reader);
            while (token.HasValue)
            {
                ConditionToken t = token.Value;

                if (t.Type == ConditionToken.TokenType.Symbol)
                    output.Enqueue(t);
                else if (t.Type == ConditionToken.TokenType.LeftBracket)
                    operators.Push(t);
                else if (t.Type == ConditionToken.TokenType.RightBracket)
                {
                    ConditionToken n = operators.Pop();
                    while (n.Type != ConditionToken.TokenType.LeftBracket)
                    {
                        if (operators.Count == 0)
                        {
                            // Not enough operators
                            throw new ConditionFormatException("Syntax error", 0, 0);
                        }

                        output.Enqueue(n);
                        n = operators.Pop();
                    }
                    if (operators.Count > 0 && operators.Peek() == ConditionToken.AND)
                        output.Enqueue(operators.Pop());
                }
                else if (t == ConditionToken.AND)
                {
                    while (operators.Count > 0 && (operators.Peek() == ConditionToken.AND || operators.Peek() == ConditionToken.OR))
                    {
                        output.Enqueue(operators.Pop());
                    }
                    operators.Push(t);
                }
                else if (t == ConditionToken.OR)
                {
                    while (operators.Count > 0 &&
                        (operators.Peek().Type == ConditionToken.TokenType.Operator && operators.Peek().Operator == ConditionToken.OperatorType.OR))
                    {
                        output.Enqueue(operators.Pop());
                    }
                    operators.Push(t);
                }

                token = ReadToken(reader);
            }

            while (operators.Count > 0)
                output.Enqueue(operators.Pop());

            // Convert to tree
            Queue<ConditionToken> reversed = new Queue<ConditionToken>(output.Reverse());
            return (ParseToken(reversed) as IConditionTreeItem);
        }

        private ConditionToken? ReadToken(PositioningReader r)
        {
            int position = r.CharPos;

            int c = r.Read();
            if (c == -1)
                return null;

            if (c == '|')
                return ConditionToken.OR;
            else if (c == ',')
                return ConditionToken.AND;
            else if (c == '(')
                return ConditionToken.LeftBracket;
            else if (c == ')')
                return ConditionToken.RightBracket;
            else
            {
                StringBuilder b = new StringBuilder(((char)c).ToString());
                int next = r.Peek();
                while (next != -1 && next != ',' && next != '|' && next != '(' && next != ')')
                {
                    b.Append((char)r.Read());
                    next = r.Peek();
                }
                return new ConditionToken(b.ToString(), position);
            }
        }

        private IConditionTreeItem ParseToken(Queue<ConditionToken> r)
        {
            if (r.Count == 0)
                return ConditionTree.Empty;

            ConditionToken t = r.Dequeue();
            if (t.Type == ConditionToken.TokenType.Symbol)
                return ParseLeaf(t);
            else if (t.Type == ConditionToken.TokenType.Operator && t.Operator == ConditionToken.OperatorType.AND)
            {
                IConditionTreeItem right = ParseToken(r);
                IConditionTreeItem left = ParseToken(r);

                return new ConditionTree(
                    ConditionTree.ConditionOperator.AND,
                    left,
                    right);
            }
            else if (t.Type == ConditionToken.TokenType.Operator && t.Operator == ConditionToken.OperatorType.OR)
            {
                IConditionTreeItem right = ParseToken(r);
                IConditionTreeItem left = ParseToken(r);

                return new ConditionTree(
                    ConditionTree.ConditionOperator.OR,
                    left,
                    right);
            }
            else
                throw new ArgumentException("Invalid queue.", "r");
        }

        private ConditionTreeLeaf ParseLeaf(ConditionToken token)
        {
            string value = token.Symbol;

            ConditionType type;
            if (value.IndexOf("$") != -1)
                type = ConditionType.Property;
            else if ((ParseFormat.StatesUnderscored && value.IndexOf("_") <= 1 && value.IndexOf("_") != -1)
                || (!ParseFormat.StatesUnderscored && value.IndexOf("_") == -1))
                type = ConditionType.State;
            else
                throw new ConditionFormatException("Illegal syntax for property or state", token.Position, token.Symbol.Length);

            ConditionComparison comparisonType = ConditionComparison.Equal;
            object compareTo = true;

            Regex opCheck = new Regex(@"(==|\[gt\]|\[lt\]|\[lteq\]|\[gteq\]|!=)");
            Match opMatch = opCheck.Match(value);
            if (opMatch.Success)
            {
                int compareToIndex = opMatch.Index + opMatch.Length;
                string compareToStr = value.Substring(compareToIndex);

                switch (opMatch.Value)
                {
                    case @"[gt]":
                        comparisonType = ConditionComparison.Greater;
                        compareTo = double.Parse(compareToStr);
                        break;
                    case @"[gteq]":
                        comparisonType = ConditionComparison.GreaterOrEqual;
                        compareTo = double.Parse(compareToStr);
                        break;
                    case @"[lt]":
                        comparisonType = ConditionComparison.Less;
                        compareTo = double.Parse(compareToStr);
                        break;
                    case @"[lteq]":
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

            string name = value;
            if (opMatch.Success)
                name = value.Substring(0, opMatch.Index);
            string variableName = Regex.Match(name, "\\$[a-zA-Z0-9]+").Value.Replace("$", "");

            // Check for illegal characters in variable name
            if (type == ConditionType.Property && name.Replace("$", "")  != variableName)
                throw new ConditionFormatException(String.Format("Illegal character in variable '{0}'", name), token.Position, token.Symbol.Length);

            if (type == ConditionType.State)
                variableName = name.Replace("!", "");

            if (type == ConditionType.State && !legalStateNames.Contains(variableName))
                throw new ConditionFormatException(String.Format("Unknown component state '{0}'", variableName), token.Position, token.Symbol.Length);

            return new ConditionTreeLeaf(type, variableName, comparisonType, compareTo);
        }

    }
}
