using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.Circuit
{
    public class TypeDescriptionComponentType : ComponentType
    {
        public TypeDescriptionComponentType(Guid id, ComponentType baseType)
            : this(id,  baseType.Collection, baseType.CollectionItem)
        {
        }

        public TypeDescriptionComponentType(Guid id, Uri collection, string collectionItem)
            : base(collection, collectionItem)
        {
            Id = id;
        }

        public Guid Id { get; }

        public override bool Equals(object obj)
        {
            return obj is TypeDescriptionComponentType type &&
                   base.Equals(obj) &&
                   Id.Equals(type.Id);
        }

        public override int GetHashCode()
        {
            var hashCode = 1545243542;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"{Id} ({CollectionItem})";
        }

        public static bool operator ==(TypeDescriptionComponentType left, TypeDescriptionComponentType right)
        {
            return EqualityComparer<TypeDescriptionComponentType>.Default.Equals(left, right);
        }

        public static bool operator !=(TypeDescriptionComponentType left, TypeDescriptionComponentType right)
        {
            return !(left == right);
        }
    }
}
