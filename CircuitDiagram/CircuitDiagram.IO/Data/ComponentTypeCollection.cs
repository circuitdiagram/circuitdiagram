using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    /// <summary>
    /// Represents a component collection that defines a number of components
    /// that are identified by their name.
    /// </summary>
    public sealed class ComponentTypeCollection
    {
        /// <summary>
        /// Creates a new instance from the specified URI.
        /// </summary>
        /// <param name="value">The URI that uniquely identifies the colleciton.</param>
        public ComponentTypeCollection(Uri value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the URI that represents this component type collection.
        /// </summary>
        public Uri Value { get; }

        private bool Equals(ComponentTypeCollection other)
        {
            return Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ComponentTypeCollection && Equals((ComponentTypeCollection)obj);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public static bool operator ==(ComponentTypeCollection left, ComponentTypeCollection right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ComponentTypeCollection left, ComponentTypeCollection right)
        {
            return !Equals(left, right);
        }
    }
}
