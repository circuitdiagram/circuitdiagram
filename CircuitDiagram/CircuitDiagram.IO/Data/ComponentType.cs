using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    /// <summary>
    /// Represents the type of a component. This class is immutable.
    /// </summary>
    public sealed class ComponentType
    {
        /// <summary>
        /// Creates a new componen type.
        /// </summary>
        /// <param name="id">The unique identifier for this component type.</param>
        /// <param name="collection">The collection this component belongs to, or null if does not belong to one.</param>
        /// <param name="item">The item this component type implements within the collection.</param>
        /// <param name="name">The name of this component type.</param>
        /// <param name="connectionNames">The connection names for this component type.</param>
        public ComponentType(Guid id,
                             ComponentTypeCollection collection,
                             ComponentTypeCollectionItem item,
                             ComponentName name,
                             IEnumerable<ConnectionName> connectionNames,
                             IEnumerable<ComponentConfiguration> configurations)
        {
            Id = id;
            Collection = collection;
            Name = name;
            ConnectionNames = connectionNames.ToList();
            Configurations = configurations.ToList();
        }

        /// <summary>
        /// Gets the unique identifier for this component type.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the collection this component type belongs to.
        /// </summary>
        public ComponentTypeCollection Collection { get; }

        /// <summary>
        /// Gets the item this component type implements within the collection.
        /// </summary>
        public ComponentTypeCollectionItem CollectionItem { get; }

        /// <summary>
        /// Gets the name of this component type.
        /// </summary>
        public ComponentName Name { get; }

        /// <summary>
        /// Gets a value indicating whether this is a standard component type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Standard component types are those that belong to a collection.
        /// For example, the 'resistor' and 'capacitor' components belong to
        /// <see cref="http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/components/common">the standard components collection</see>.
        /// </para>
        /// </remarks>
        public bool IsStandard => Collection != null;

        public IReadOnlyCollection<ConnectionName> ConnectionNames { get; }

        public IReadOnlyCollection<ComponentConfiguration> Configurations { get; }

        private bool Equals(ComponentType other)
        {
            return Id.Equals(other.Id) && Equals(Collection, other.Collection) && Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ComponentType && Equals((ComponentType)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (Collection?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(ComponentType left, ComponentType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ComponentType left, ComponentType right)
        {
            return !Equals(left, right);
        }
    }
}
