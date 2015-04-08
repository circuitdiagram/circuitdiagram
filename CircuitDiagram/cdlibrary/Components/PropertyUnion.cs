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

using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components
{
    /// <summary>
    /// Stores property values as .NET types, which can be either strings, decimal numbers or booleans.
    /// </summary>
    public sealed class PropertyUnion : IComparable<PropertyUnion>
    {
        private readonly bool booleanValue;
        private readonly double doubleValue;
        private readonly string stringValue;

        private readonly PropertyUnionType internalType;

        internal PropertyUnionType InternalType { get { return internalType; } }

        /// <summary>
        /// Gets the value of this union.
        /// </summary>
        public object Value
        {
            get
            {
                switch(internalType)
                {
                    case PropertyUnionType.Boolean:
                        return booleanValue;
                    case PropertyUnionType.Double:
                        return doubleValue;
                    default:
                        return stringValue;
                }
            }
        }

        public PropertyUnion(string value)
        {
            stringValue = value;
            internalType = PropertyUnionType.String;
        }

        public PropertyUnion(double value)
        {
            doubleValue = value;
            internalType = PropertyUnionType.Double;
        }

        public PropertyUnion(bool value)
        {
            booleanValue = value;
            internalType = PropertyUnionType.Boolean;
        }

        public PropertyUnion(string value, PropertyUnionType parseAs)
        {
            switch(parseAs)
            {
                case PropertyUnionType.Boolean:
                    booleanValue = bool.Parse(value);
                    break;
                case PropertyUnionType.Double:
                    doubleValue = double.Parse(value);
                    break;
                case PropertyUnionType.String:
                    stringValue = value;
                    break;
            }
            internalType = parseAs;
        }

        public PropertyUnion(string value, PropertyType parseAs)
        {
            switch (parseAs)
            {
                case PropertyType.Boolean:
                    booleanValue = bool.Parse(value);
                    internalType = PropertyUnionType.Boolean;
                    break;
                case PropertyType.Decimal:
                    doubleValue = double.Parse(value);
                    internalType = PropertyUnionType.Double;
                    break;
                case PropertyType.Enum:
                    stringValue = value;
                    internalType = PropertyUnionType.String;
                    break;
                case PropertyType.Integer:
                    doubleValue = (double)int.Parse(value);
                    internalType = PropertyUnionType.Double;
                    break;
                case PropertyType.String:
                    stringValue = value;
                    internalType = PropertyUnionType.String;
                    break;
            }
        }

        public int CompareTo(PropertyUnion other)
        {
            // Decimals
            if (internalType == PropertyUnionType.Double && other.internalType == PropertyUnionType.Double)
                return doubleValue.CompareTo(other.doubleValue);

            // Booleans
            if (internalType == PropertyUnionType.Boolean && other.internalType == PropertyUnionType.Boolean)
                return booleanValue.CompareTo(other.booleanValue);

            // Strings
            if (internalType == PropertyUnionType.String && other.internalType == PropertyUnionType.String)
                return stringValue.CompareTo(other.stringValue);

            if (Value.ToString() == other.Value.ToString())
                return 0; // Equal
            else
                return -1; // Not equal
        }

        public bool IsEmpty()
        {
            return String.IsNullOrEmpty(Value.ToString());
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to ConditionUnion return false.
            PropertyUnion o = obj as PropertyUnion;
            if ((System.Object)o == null)
            {
                return false;
            }

            // Compare values directly if same type
            if (internalType == o.internalType)
            {
                switch(internalType)
                {
                    case PropertyUnionType.Boolean:
                        return booleanValue == o.booleanValue;
                    case PropertyUnionType.Double:
                        return doubleValue == o.doubleValue;
                    case PropertyUnionType.String:
                        return stringValue == o.stringValue;
                }
            }

            // Else compare as string
            return Value.ToString() == o.Value.ToString();
        }

        public override int GetHashCode()
        {
            switch (internalType)
            {
                case PropertyUnionType.Boolean:
                    return booleanValue.GetHashCode();
                case PropertyUnionType.Double:
                    return doubleValue.GetHashCode();
                default:
                    return stringValue.GetHashCode();
            }
        }
    }

    public enum PropertyUnionType
    {
        String,
        Double,
        Boolean
    }
}
