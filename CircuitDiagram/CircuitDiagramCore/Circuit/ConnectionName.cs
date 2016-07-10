using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit.Internal;

namespace CircuitDiagram.Circuit
{
    public class ConnectionName : ObjValue<string>
    {
        public ConnectionName(string value) : base(value)
        {
        }

        protected bool Equals(ConnectionName other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
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
