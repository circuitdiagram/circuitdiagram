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
using System.IO;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components.Description
{
    /// <summary>
    /// Parses a condition in the form "($prop==a||$prop2==b)"
    /// </summary>
    public class ConditionParser
    {
        private static ConditionToken? ReadToken(StringReader r)
        {
            int c = r.Read();
            if (c == -1)
                return null;

            if (c == '&' || c == '|')
            {
                int d = r.Read();
                if (c == '&' && d == '&')
                    return ConditionToken.AND;
                else if (c == '|' && d == '|')
                    return ConditionToken.OR;
                else
                    throw new ArgumentException("Syntax error.", "r");
            }
            else if (c == '(')
                return ConditionToken.LeftBracket;
            else if (c == ')')
                return ConditionToken.RightBracket;
            else
            {
                StringBuilder b = new StringBuilder(((char)c).ToString());
                int next = r.Peek();
                while (next != -1 && next != '&' && next != '|' && next != '(' && next != ')')
                {
                    b.Append((char)r.Read());
                    next = r.Peek();
                }
                return new ConditionToken(b.ToString());
            }
        }

        internal static IConditionTreeItem ParseToken(Queue<ConditionToken> r)
        {
            if (r.Count == 0)
                return ConditionTree.Empty;

            ConditionToken t = r.Dequeue();
            if (t.Type == ConditionToken.TokenType.Symbol)
                return ConditionTreeLeaf.ParseV1_1(t.Symbol);
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

        public static IConditionTreeItem Parse(string conditions)
        {
            // Tokenize

            Queue<ConditionToken> output = new Queue<ConditionToken>();
            Stack<ConditionToken> operators = new Stack<ConditionToken>();

            StringReader reader = new StringReader(conditions);
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

        public static IConditionTreeItem ConvertLegacyConditions(Stack<ConditionTreeLeaf> andList)
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
    }
}
