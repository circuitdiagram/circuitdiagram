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

        public IConditionTreeItem Parse(string input, ParseContext context)
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
            return (ParseToken(reversed, context) as IConditionTreeItem);
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

        private IConditionTreeItem ParseToken(Queue<ConditionToken> r, ParseContext context)
        {
            if (r.Count == 0)
                throw new ConditionFormatException("Invalid condition", 0, 0);

            ConditionToken t = r.Dequeue();
            if (t.Type == ConditionToken.TokenType.Symbol)
                return ParseLeaf(t, context);
            else if (t.Type == ConditionToken.TokenType.Operator && t.Operator == ConditionToken.OperatorType.AND)
            {
                IConditionTreeItem right = ParseToken(r, context);
                IConditionTreeItem left = ParseToken(r, context);

                return new ConditionTree(
                    ConditionTree.ConditionOperator.AND,
                    left,
                    right);
            }
            else if (t.Type == ConditionToken.TokenType.Operator && t.Operator == ConditionToken.OperatorType.OR)
            {
                IConditionTreeItem right = ParseToken(r, context);
                IConditionTreeItem left = ParseToken(r, context);

                return new ConditionTree(
                    ConditionTree.ConditionOperator.OR,
                    left,
                    right);
            }
            else
                throw new ArgumentException("Invalid queue.", "r");
        }

        private ConditionTreeLeaf ParseLeaf(ConditionToken token, ParseContext context)
        {
            bool isNegated;
            bool isState;
            string property;
            string comparisonStr;
            string compareToStr;
            SplitLeaf(token, out isNegated, out isState, out property, out comparisonStr, out compareToStr);

            if (compareToStr == String.Empty)
                compareToStr = "true"; // Implicit true

            ConditionComparison comparison;
            switch(comparisonStr)
            {
                case "==":
                    comparison = ConditionComparison.Equal;
                    break;
                case "!=":
                    comparison = ConditionComparison.NotEqual;
                    break;
                case "[gt]":
                    comparison = ConditionComparison.Greater;
                    break;
                case "[lt]":
                    comparison = ConditionComparison.Less;
                    break;
                case "[gteq]":
                    comparison = ConditionComparison.GreaterOrEqual;
                    break;
                case "[lteq]":
                    comparison = ConditionComparison.LessOrEqual;
                    break;
                default:
                    comparison = ConditionComparison.Equal;
                    break;
            }

            if (isNegated && comparison == ConditionComparison.Equal)
                comparison = ConditionComparison.NotEqual;
            else if (isNegated)
            {
                // Operator cannot be negated
                throw new ConditionFormatException("Comparison cannot be negated (illegal '!')", token.Position, token.Symbol.Length);
            }

            if (isState)
            {
                if (!legalStateNames.Contains(property))
                    throw new ConditionFormatException(String.Format("Unknown component state '{0}'", property), token.Position, token.Symbol.Length);

                return new ConditionTreeLeaf(ConditionType.State, property, comparison, new PropertyUnion(compareToStr, PropertyUnionType.Boolean));
            }
            else
            {
                PropertyUnionType propertyType;
                if (!context.PropertyTypes.TryGetValue(property, out propertyType))
                    throw new ConditionFormatException(String.Format("Unknown property '{0}'", property), token.Position, token.Symbol.Length);

                return new ConditionTreeLeaf(ConditionType.Property, property, comparison, new PropertyUnion(compareToStr, propertyType));
            }
        }

        public static void SplitLeaf(ConditionToken token, out bool isNegated, out bool isState, out string property, out string op, out string compareTo)
        {
            string rIsNegated = @"(!?)";
            string rIsState = @"(\$?)";
            string rProperty = @"([a-zA-Z]+[a-zA-Z0-9_]*)";
            string rOperator = @"((==|\[gt\]|\[lt\]|\[lteq\]|\[gteq\]|!=)";
            string rCompareTo = @"([a-zA-Z0-9_.]+))?";
            Regex regex = new Regex(String.Format("{0}{1}{2}{3}{4}", rIsNegated, rIsState, rProperty, rOperator, rCompareTo));
            Match match = regex.Match(token.Symbol);

            if (!match.Success)
                throw new ConditionFormatException("Invalid syntax", token.Position, token.Symbol.Length);

            isNegated = match.Groups[1].Value.Length == 1;
            isState = match.Groups[2].Value.Length == 0;
            property = match.Groups[3].Value;
            op = match.Groups[5].Value;
            compareTo = match.Groups[6].Value;
        }
    }
}
