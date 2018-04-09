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
    public class ComponentType
    {
        public static readonly Uri UnknownCollection = new Uri("http://schemas.circuit-diagram.org/unknown");

        public ComponentType(Uri collection, string collectionItem)
        {
            Collection = collection ?? throw new ArgumentNullException(nameof(collection));
            CollectionItem = collectionItem ?? throw new ArgumentNullException(nameof(collectionItem));
        }

        /// <summary>
        /// Gets the collection this component type belongs to.
        /// </summary>
        public Uri Collection { get; }

        /// <summary>
        /// Gets the item this component type implements within the collection.
        /// </summary>
        public string CollectionItem { get; }

        protected bool Equals(ComponentType other)
        {
            if (Collection == UnknownCollection || other.Collection == UnknownCollection)
                return false;

            return Collection.Equals(other.Collection) && string.Equals(CollectionItem, other.CollectionItem);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as ComponentType;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Collection.GetHashCode() * 397) ^ CollectionItem.GetHashCode();
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

        public static ComponentType Unknown(string name)
        {
            return new ComponentType(UnknownCollection, name);
        }
    }
}
