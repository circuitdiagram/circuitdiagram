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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components.Conditions
{
    public class ConditionTree : IConditionTreeItem
    {
        /// <summary>
        /// Represents a condition tree with no conditions, that is always met.
        /// </summary>
        public static readonly IConditionTreeItem Empty = new ConditionTreeLeaf();

        public enum ConditionOperator : ushort
        {
            AND = 1,
            OR = 2
        }

        private static string ConditionOperatorToString(ConditionOperator op)
        {
            switch(op)
            {
                case ConditionOperator.AND:
                    return ",";
                case ConditionOperator.OR:
                    return "|";
                default:
                    return "??";
            }
        }

        public ConditionOperator Operator { get; private set; }
        public IConditionTreeItem Left { get; private set; }
        public IConditionTreeItem Right { get; private set; }

        public ConditionTree(ConditionOperator op, IConditionTreeItem left, IConditionTreeItem right)
        {
            this.Operator = op;
            this.Left = left;
            this.Right = right;
        }

        public bool IsMet(Component component)
        {
            if (this.Operator == ConditionOperator.AND)
                return Left.IsMet(component) && Right.IsMet(component);
            else if (this.Operator == ConditionOperator.OR)
                return Left.IsMet(component) || Right.IsMet(component);
            else
                return false; // Unknown operator
        }

        public bool ConditionsAreMet(Component component)
        {
            return this.IsMet(component);
        }

        public override string ToString()
        {
            return this.Left.ToString() + ConditionOperatorToString(this.Operator) + this.Right.ToString();
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to ConditionTree return false.
            ConditionTree o = obj as ConditionTree;
            if ((System.Object)o == null)
            {
                return false;
            }

            // Return true if the fields match:
            return Operator.Equals(o.Operator)
                && Left.Equals(o.Left)
                && Right.Equals(o.Right);
        }

        public override int GetHashCode()
        {
            return Operator.GetHashCode()
                ^ Left.GetHashCode()
                ^ Right.GetHashCode();
        }
    }
}
