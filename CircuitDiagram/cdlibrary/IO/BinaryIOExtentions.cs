// BinaryIOExtensions.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012-2014  Sam Fisher
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
using CircuitDiagram.Components;
using CircuitDiagram.Components.Description;

namespace CircuitDiagram.IO
{
    public static class BinaryIOExtentions
    {
        public static void Write(this System.IO.BinaryWriter writer, ComponentPoint value)
        {
            writer.Write((uint)value.RelativeToX);
            writer.Write((uint)value.RelativeToY);
            writer.Write(value.Offset.X);
            writer.Write(value.Offset.Y);
        }

        public static ComponentPoint ReadComponentPoint(this System.IO.BinaryReader reader)
        {
            ComponentPosition relX = (ComponentPosition)reader.ReadUInt32();
            ComponentPosition relY = (ComponentPosition)reader.ReadUInt32();
            double offsetX = reader.ReadDouble();
            double offsetY = reader.ReadDouble();
            return new ComponentPoint(relX, relY, new System.Windows.Vector(offsetX, offsetY));
        }

        public static void WriteType(this System.IO.BinaryWriter writer, object value, bool isEnum = false)
        {
            Type valueType = value.GetType();
            if (valueType == typeof(string) && !isEnum)
            {
                writer.Write((int)BinaryType.String);
                writer.Write(value as string);
            }
            else if (valueType == typeof(int))
            {
                writer.Write((int)BinaryType.Int);
                writer.Write((int)value);
            }
            else if (valueType == typeof(double))
            {
                writer.Write((int)BinaryType.Double);
                writer.Write((double)value);
            }
            else if (valueType == typeof(bool))
            {
                writer.Write((int)BinaryType.Bool);
                writer.Write((bool)value);
            }
            else if (valueType == typeof(string) && isEnum)
            {
                writer.Write((int)BinaryType.Enum);
                writer.Write(value as string);
            }
            else
            {
                writer.Write((int)BinaryType.Unknown);
            }
        }

        public static object ReadType(this System.IO.BinaryReader reader, out BinaryType type)
        {
            type = (BinaryType)reader.ReadInt32();
            if (type == BinaryType.String)
                return reader.ReadString();
            else if (type == BinaryType.Int)
                return reader.ReadInt32();
            else if (type == BinaryType.Double)
                return reader.ReadDouble();
            else if (type == BinaryType.Bool)
                return reader.ReadBoolean();
            else if (type == BinaryType.Enum)
                return reader.ReadString();
            return null;
        }

        public static Type BinaryTypeToType(BinaryType type)
        {
            switch (type)
            {
                case BinaryType.String:
                    return typeof(string);
                case BinaryType.Int:
                    return typeof(int);
                case BinaryType.Double:
                    return typeof(double);
                case BinaryType.Bool:
                    return typeof(bool);
                case BinaryType.Enum:
                    return typeof(string);
                default:
                    return typeof(string);
            }
        }

        public static BinaryType TypeToBinaryType(Type type)
        {
            if (type == typeof(string))
                return BinaryType.String;
            else if (type == typeof(int))
                return BinaryType.Int;
            else if (type == typeof(double))
                return BinaryType.Double;
            else if (type == typeof(bool))
                return BinaryType.Bool;
            else
                return BinaryType.Unknown;
        }

        public static void Write(this System.IO.BinaryWriter writer, IConditionTreeItem value)
        {
            if (value == Condition.Empty)
            {
                writer.Write((byte)0); // 0 for empty
            }
            else if (value is ConditionTree)
            {
                writer.Write((byte)1); // 1 for tree
             
                var tree = value as ConditionTree;
                writer.Write((ushort)tree.Operator);
                writer.Write(tree.Left);
                writer.Write(tree.Right);
            }
            else if (value is Condition)
            {
                var condition = value as Condition;
                writer.Write((byte)2); // 0 for condition
                writer.Write((int)condition.Type);
                writer.Write((int)condition.Comparison);
                writer.Write(condition.VariableName);
                writer.WriteType(condition.CompareTo);
            }
        }

        public static IConditionTreeItem ReadConditionTree(this System.IO.BinaryReader reader)
        {
            byte type = reader.ReadByte();
            if (type == 0)
            {
                // Empty
                return Condition.Empty;
            }
            else if (type == 1)
            {
                // Tree
                ConditionTree.ConditionOperator op = (ConditionTree.ConditionOperator)reader.ReadUInt16();
                IConditionTreeItem left = reader.ReadConditionTree();
                IConditionTreeItem right = reader.ReadConditionTree();
                return new ConditionTree(op, left, right);
            }
            else if (type == 2)
            {
                ConditionType conditionType = (ConditionType)reader.ReadInt32();
                ConditionComparison comparison = (ConditionComparison)reader.ReadInt32();
                string variableName = reader.ReadString();
                BinaryType binType;
                object compareTo = reader.ReadType(out binType);
                return new Condition(conditionType, variableName, comparison, compareTo);
            }
            else
                throw new System.IO.InvalidDataException();
        }

        public static IConditionTreeItem ReadConditionCollection(this System.IO.BinaryReader reader)
        {
            Stack<Condition> andList = new Stack<Condition>();
            int numConditions = reader.ReadInt32();
            for (int l = 0; l < numConditions; l++)
            {
                ConditionType conditionType = (ConditionType)reader.ReadInt32();
                ConditionComparison comparison = (ConditionComparison)reader.ReadInt32();
                string variableName = reader.ReadString();
                BinaryType binType;
                object compareTo = reader.ReadType(out binType);
                andList.Push(new Condition(conditionType, variableName, comparison, compareTo));
            }
            return ConditionParser.ConvertLegacyConditions(andList);
        }

        public static void Write(this System.IO.BinaryWriter writer, System.Windows.Point value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public static System.Windows.Point ReadPoint(this System.IO.BinaryReader reader)
        {
            return new System.Windows.Point(reader.ReadDouble(), reader.ReadDouble());
        }

        public static void WriteNullString(this System.IO.BinaryWriter writer, string value)
        {
            writer.Write((value != null ? value : ""));
        }
    }

    public enum BinaryType
    {
        Unknown = 0,
        String = 1,
        Int = 2,
        Double = 3,
        Bool = 4,
        Enum = 5
    }
}
