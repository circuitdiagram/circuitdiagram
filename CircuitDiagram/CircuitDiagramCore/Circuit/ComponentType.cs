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

        public static ComponentType Unknown(string name)
        {
            return new ComponentType(UnknownCollection, name);
        }

        public override bool Equals(object obj)
        {
            return obj is ComponentType type &&
                   EqualityComparer<Uri>.Default.Equals(Collection, type.Collection) &&
                   CollectionItem == type.CollectionItem;
        }

        public override int GetHashCode()
        {
            var hashCode = -932068893;
            hashCode = hashCode * -1521134295 + EqualityComparer<Uri>.Default.GetHashCode(Collection);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CollectionItem);
            return hashCode;
        }

        public override string ToString()
        {
            return $"{Collection}:{CollectionItem}";
        }

        public static bool operator ==(ComponentType left, ComponentType right)
        {
            return EqualityComparer<ComponentType>.Default.Equals(left, right);
        }

        public static bool operator !=(ComponentType left, ComponentType right)
        {
            return !(left == right);
        }
    }
}
