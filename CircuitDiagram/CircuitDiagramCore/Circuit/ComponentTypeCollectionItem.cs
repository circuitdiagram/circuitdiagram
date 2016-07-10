using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Circuit
{
    public sealed class ComponentTypeCollectionItem
    {
        public ComponentTypeCollectionItem(string item)
        {
            if (!ValidateName(item))
                throw new ArgumentException("Supplied string is not a valid collection item name.", nameof(item));
            
            Item = item;
        }

        public string Item { get; }
        
        private static bool ValidateName(string name)
        {
            return !name.Contains(" ") && // No whitespace
                   name.Length > 1;       // Not empty
        }

        public override string ToString()
        {
            return Item;
        }

        private bool Equals(ComponentTypeCollectionItem other)
        {
            return string.Equals(Item, other.Item);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ComponentTypeCollectionItem)obj);
        }

        public override int GetHashCode()
        {
            return (Item != null ? Item.GetHashCode() : 0);
        }

        public static bool operator ==(ComponentTypeCollectionItem left, ComponentTypeCollectionItem right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ComponentTypeCollectionItem left, ComponentTypeCollectionItem right)
        {
            return !Equals(left, right);
        }
    }
}
