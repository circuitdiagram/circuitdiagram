using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.Circuit
{
    public class ComponentTypeEqualityComparer : IEqualityComparer<ComponentType>
    {
        public static ComponentTypeEqualityComparer Instance { get; } = new ComponentTypeEqualityComparer();

        public bool Equals(ComponentType x, ComponentType y)
        {
            return x.Collection == y.Collection && x.CollectionItem == y.CollectionItem;
        }

        public int GetHashCode(ComponentType obj)
        {
            var hashCode = -932068893;
            hashCode = hashCode * -1521134295 + EqualityComparer<Uri>.Default.GetHashCode(obj.Collection);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.CollectionItem);
            return hashCode;
        }
    }
}
