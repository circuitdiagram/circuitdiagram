using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    public class CollectionType
    {
        public CollectionType(ComponentTypeCollection collection,
                              ComponentTypeCollectionItem collectionItem)
        {
            Collection = collection;
            CollectionItem = collectionItem;
        }

        /// <summary>
        /// Gets the collection this component type belongs to.
        /// </summary>
        public ComponentTypeCollection Collection { get; }

        /// <summary>
        /// Gets the item this component type implements within the collection.
        /// </summary>
        public ComponentTypeCollectionItem CollectionItem { get; }

        protected bool Equals(CollectionType other)
        {
            return Equals(Collection, other.Collection) && Equals(CollectionItem, other.CollectionItem);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as CollectionType;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Collection?.GetHashCode() ?? 0) * 397) ^ (CollectionItem?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(CollectionType left, CollectionType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CollectionType left, CollectionType right)
        {
            return !Equals(left, right);
        }
    }
}
