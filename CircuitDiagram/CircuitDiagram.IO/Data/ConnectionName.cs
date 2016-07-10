using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    public class ConnectionName
    {
        public ConnectionName(string value)
        {
            if (value == null)
                throw new ArgumentException("Value cannot be null.", nameof(value));

            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }

        protected bool Equals(ConnectionName other)
        {
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConnectionName)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(ConnectionName left, ConnectionName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ConnectionName left, ConnectionName right)
        {
            return !Equals(left, right);
        }

        public static implicit operator ConnectionName(string value)
        {
            return new ConnectionName(value);
        }
    }
}
