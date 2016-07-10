using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Circuit.Internal
{
    /// <summary>
    /// Encapsulates a non-null value.
    /// </summary>
    /// <typeparam name="T">Type of the value to store.</typeparam>
    public abstract class ObjValue<T>
    {
        protected ObjValue(T value)
        {
            if (value == null)
                throw new ArgumentException("Value cannot be null.", nameof(value));

            Value = value;
        }

        public T Value { get; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
