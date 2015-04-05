// ConditionToken.cs
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
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components.Description
{
    public struct ConditionToken
    {
        public enum TokenType : byte
        {
            Symbol = 1,
            LeftBracket = 2,
            RightBracket = 3,
            Operator = 4
        }

        public enum OperatorType : byte
        {
            None = 0,
            AND = 1,
            OR = 2
        }

        public TokenType Type { get; set; }
        public OperatorType Operator { get; set; }
        public string Symbol { get; set; }

        public ConditionToken(TokenType type)
            : this()
        {
            Type = type;
            Operator = OperatorType.None;
        }

        public ConditionToken(OperatorType op)
            : this()
        {
            Type = TokenType.Operator;
            Operator = op;
        }

        public ConditionToken(string symbol)
            : this()
        {
            Type = TokenType.Symbol;
            Operator = OperatorType.None;
            Symbol = symbol;
        }

        public static ConditionToken LeftBracket = new ConditionToken(TokenType.LeftBracket);
        public static ConditionToken RightBracket = new ConditionToken(TokenType.RightBracket);
        public static ConditionToken AND = new ConditionToken(OperatorType.AND);
        public static ConditionToken OR = new ConditionToken(OperatorType.OR);

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            ConditionToken other = (ConditionToken)obj;
            return (this.Operator == other.Operator &&
                this.Type == other.Type &&
                this.Symbol == other.Symbol);
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode() + this.Operator.GetHashCode();
        }

        public static bool operator ==(ConditionToken t1, ConditionToken t2)
        {
            return t1.Equals(t2);
        }

        public static bool operator !=(ConditionToken t1, ConditionToken t2)
        {
            return !t1.Equals(t2);
        }

        public override string ToString()
        {
            if (this.Type == TokenType.Symbol)
                return this.Symbol;
            else
                return this.Type.ToString();
        }
    }
}
