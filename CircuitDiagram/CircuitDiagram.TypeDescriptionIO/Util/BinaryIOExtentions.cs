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
using CircuitDiagram.Circuit;
using CircuitDiagram.Primitives;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.Conditions;

namespace CircuitDiagram.TypeDescriptionIO.Util
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
            return new ComponentPoint(relX, relY, new Vector(offsetX, offsetY));
        }

        public static void WriteType(this System.IO.BinaryWriter writer, PropertyValue value, bool isEnum = false)
        {
            var valueType = value.PropertyType;
            if (valueType == PropertyValue.Type.String && !isEnum)
            {
                writer.Write((int)BinaryType.String);
                writer.Write(value.StringValue);
            }
            else if (valueType == PropertyValue.Type.Numeric)
            {
                writer.Write((int)BinaryType.Double);
                writer.Write((double)value.NumericValue);
            }
            else if (valueType == PropertyValue.Type.Boolean)
            {
                writer.Write((int)BinaryType.Bool);
                writer.Write((bool)value.BooleanValue);
            }
            else if (valueType == PropertyValue.Type.String && isEnum)
            {
                writer.Write((int)BinaryType.Enum);
                writer.Write(value.StringValue);
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

        public static PropertyType BinaryTypeToPropertyType(BinaryType type)
        {
            switch (type)
            {
                case BinaryType.Int:
                    return PropertyType.Integer;
                case BinaryType.Double:
                    return PropertyType.Decimal;
                case BinaryType.Bool:
                    return PropertyType.Boolean;
                case BinaryType.Enum:
                    return PropertyType.Enum;
                default:
                    return PropertyType.String;
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
            if (value == ConditionTree.Empty)
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
            else if (value is ConditionTreeLeaf)
            {
                var condition = value as ConditionTreeLeaf;
                writer.Write((byte)2); // 2 for condition
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
                return ConditionTree.Empty;
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

                return new ConditionTreeLeaf(conditionType, variableName, comparison, binType.ToPropertyUnion(compareTo));
            }
            else
                throw new System.IO.InvalidDataException();
        }

        public static IConditionTreeItem ReadConditionCollection(this System.IO.BinaryReader reader)
        {
            Stack<ConditionTreeLeaf> andList = new Stack<ConditionTreeLeaf>();
            int numConditions = reader.ReadInt32();
            for (int l = 0; l < numConditions; l++)
            {
                ConditionType conditionType = (ConditionType)reader.ReadInt32();
                ConditionComparison comparison = (ConditionComparison)reader.ReadInt32();
                string variableName = reader.ReadString();
                BinaryType binType;
                object compareTo = reader.ReadType(out binType);
                andList.Push(new ConditionTreeLeaf(conditionType, variableName, comparison, binType.ToPropertyUnion(compareTo)));
            }

            return LegacyConditionParser.AndListToTree(andList);
        }

        public static void Write(this System.IO.BinaryWriter writer, Point value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public static Point ReadPoint(this System.IO.BinaryReader reader)
        {
            return new Point(reader.ReadDouble(), reader.ReadDouble());
        }

        public static void WriteNullString(this System.IO.BinaryWriter writer, string value)
        {
            writer.Write((value != null ? value : ""));
        }

        public static PropertyValue ToPropertyUnion(this BinaryType binType, object value)
        {
            switch(binType)
            {
                case BinaryType.Bool:
                    return new PropertyValue((bool)value);
                case BinaryType.Double:
                    return new PropertyValue((double)value);
                case BinaryType.Int:
                    return new PropertyValue((int)value);
                default:
                    return new PropertyValue((string)value);
            }
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
