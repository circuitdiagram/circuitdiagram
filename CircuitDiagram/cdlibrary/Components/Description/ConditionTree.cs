// ConditionTree.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2011-2014  Sam Fisher
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components.Description
{
    public class ConditionTree : IConditionTreeItem
    {
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
                    return "&&";
                case ConditionOperator.OR:
                    return "||";
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
    }
}
