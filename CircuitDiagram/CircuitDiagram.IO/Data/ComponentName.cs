using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    /// <summary>
    /// Represents the name of a component.
    /// </summary>
    public sealed class ComponentName
    {
        /// <summary>
        /// Creates a new component name from the specified string.
        /// </summary>
        /// <param name="value">The component name.</param>
        public ComponentName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invalid component name", nameof(value));

            Value = value;
        }

        /// <summary>
        /// Gets the string representation of the component name.
        /// </summary>
        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }

        private bool Equals(ComponentName other)
        {
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ComponentName && Equals((ComponentName)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(ComponentName left, ComponentName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ComponentName left, ComponentName right)
        {
            return !Equals(left, right);
        }

        public static implicit operator ComponentName(string value)
        {
            return new ComponentName(value);
        }
    }
}
