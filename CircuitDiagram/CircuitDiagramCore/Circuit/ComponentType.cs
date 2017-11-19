using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Circuit
{
    /// <summary>
    /// Represents the type of a component.
    /// </summary>
    public class ComponentType : CollectionType
    {
        public ComponentType(Guid? id, string name)
            : base(ComponentTypeCollection.Unknown, (ComponentTypeCollectionItem)null)
        {
            Id = id;
            Name = name;
            PropertyNames = new HashSet<PropertyName>();
            ConnectionNames = new HashSet<ConnectionName>();
            Configurations = new List<ComponentConfiguration>();
        }

        /// <summary>
        /// Creates a new component type.
        /// </summary>
        /// <param name="id">The unique identifier for this component type.</param>
        /// <param name="collection">The collection this component belongs to, or null if does not belong to one.</param>
        /// <param name="item">The item this component type implements within the collection.</param>
        /// <param name="name">The name of this component type.</param>
        /// <param name="propertyNames">The properties that can be set on this component.</param>
        /// <param name="connectionNames">The connection names for this component type.</param>
        public ComponentType(Guid? id,
                             ComponentTypeCollection collection,
                             ComponentTypeCollectionItem item,
                             ComponentName name,
                             IEnumerable<PropertyName> propertyNames,
                             IEnumerable<ConnectionName> connectionNames,
                             IEnumerable<ComponentConfiguration> configurations)
            : base(collection, item)
        {
            Id = id;
            Name = name;
            PropertyNames = propertyNames.ToHashSet();
            ConnectionNames = connectionNames.ToHashSet();
            Configurations = configurations.ToList();
        }

        /// <summary>
        /// Gets the unique identifier for this component type.
        /// </summary>
        public Guid? Id { get; }
        
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
        public bool IsStandard => Collection != null && CollectionItem != null;

        /// <summary>
        /// Gets a list of properties that can be set on this component.
        /// </summary>
        public ISet<PropertyName> PropertyNames { get; } 

        /// <summary>
        /// Gets a list of connections available on this component.
        /// </summary>
        public ISet<ConnectionName> ConnectionNames { get; }

        public ICollection<ComponentConfiguration> Configurations { get; }

        protected bool Equals(ComponentType other)
        {
            return base.Equals(other) && Id.Equals(other.Id) && Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return ((ComponentType)obj).Equals(this);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ Id.GetHashCode();
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ComponentType left, CollectionType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ComponentType left, CollectionType right)
        {
            return !(left == right);
        }
    }
}
