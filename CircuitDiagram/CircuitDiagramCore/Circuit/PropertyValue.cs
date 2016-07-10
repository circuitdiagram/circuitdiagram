using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Circuit
{
    public sealed class PropertyValue : IComparable<PropertyValue>
    {
        public PropertyValue()
        {
            PropertyType = Type.Unset;
        }

        private PropertyValue(Type type)
        {
            PropertyType = type;
        }

        private PropertyValue(string stringValue, Type type)
            : this(type)
        {
            StringValue = stringValue;
        }

        public PropertyValue(string value)
            : this(Type.String)
        {
            StringValue = value;
        }

        public PropertyValue(double value)
            : this(Type.Numeric)
        {
            NumericValue = value;
        }

        public PropertyValue(bool value)
            : this(Type.Boolean)
        {
            BooleanValue = value;
        }

        public string StringValue { get; }

        public double NumericValue { get; private set; }

        public bool BooleanValue { get; }

        public void Match(Action<string> s,
                          Action<double> n,
                          Action<bool> b)
        {
            switch (PropertyType)
            {
                case Type.Boolean:
                    b(BooleanValue);
                    return;
                case Type.Numeric:
                    n(NumericValue);
                    return;
                case Type.String:
                    s(StringValue);
                    return;
                case Type.Unknown:
                    if (StringValue != null)
                        s(StringValue);
                    return;
            }
        }

        public Type PropertyType { get; private set; }

        public int CompareTo(PropertyValue other)
        {
            var thisProperty = this;
            if (PropertyType == Type.Unknown && other.PropertyType != Type.Unknown)
            {
                // Convert the type of this property
                switch (other.PropertyType)
                {
                    case Type.String:
                        thisProperty = new PropertyValue(ToString());
                        break;
                    case Type.Numeric:
                        thisProperty = PropertyValue.Parse(ToString(), Type.Numeric);
                        break;
                    case Type.Boolean:
                        thisProperty = new PropertyValue(ToString().ToLowerInvariant() == "true");
                        break;
                }
            }

            if (thisProperty.PropertyType != other.PropertyType)
                throw new InvalidOperationException("Cannot compare property values of different types.");

            switch (thisProperty.PropertyType)
            {
                case Type.Boolean:
                    return thisProperty.BooleanValue.CompareTo(other.BooleanValue);
                case Type.Numeric:
                    return thisProperty.NumericValue.CompareTo(other.NumericValue);
                case Type.String:
                    return thisProperty.StringValue.CompareTo(other.StringValue);
                default:
                    return 0;
            }
        }

        public bool IsNumeric()
        {
            if (PropertyType == Type.Numeric)
                return true;

            double val;
            if (double.TryParse(ToString(), out val))
            {
                PropertyType = Type.Numeric;
                NumericValue = val;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            string result = string.Empty;

            Match(s => result = s,
                  n => result = n.ToString(),
                  b => result = b.ToString());

            return result;
        }

        public static PropertyValue Parse(string value, Type parseAs)
        {
            switch (parseAs)
            {
                case Type.Boolean:
                    return new PropertyValue(bool.Parse(value));
                case Type.Numeric:
                    return new PropertyValue(double.Parse(value));
                case Type.String:
                    return new PropertyValue(value);
                default:
                    return new PropertyValue();
            }
        }

        public static PropertyValue Dynamic(string value)
        {
            return new PropertyValue(value, Type.Unknown);
        }

        public enum Type
        {
            String,
            Numeric,
            Boolean,
            Unknown,
            Unset
        }
    }
}
