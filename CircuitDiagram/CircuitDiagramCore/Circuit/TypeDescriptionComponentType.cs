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

        public override string ToString()
        {
            return $"{Id} ({CollectionItem})";
        }
    }
}
