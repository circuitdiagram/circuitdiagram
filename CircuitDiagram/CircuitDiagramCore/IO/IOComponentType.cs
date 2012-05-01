// IOComponentType.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Represents the type of a component.
    /// </summary>
    public class IOComponentType
    {
        /// <summary>
        /// The collection the type of the component belongs to.
        /// </summary>
        public string Collection { get; set; }

        /// <summary>
        /// The type of component within the collection.
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// Gets a value indicating whether type is from a collection or is non-standard.
        /// </summary>
        public bool IsStandard
        {
            get { return Collection != null; }
        }

        /// <summary>
        /// The GUID of the component description that was used in the original document.
        /// </summary>
        public Guid GUID { get; set; }

        /// <summary>
        /// The name of the component description that was used in the original document.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new IOComponentType.
        /// </summary>
        private IOComponentType()
        {
            GUID = Guid.Empty;
        }

        /// <summary>
        /// Initializes a new non-standard component type.
        /// </summary>
        /// <param name="name"></param>
        public IOComponentType(string name)
        {
            Item = name;
            GUID = Guid.Empty;
        }

        /// <summary>
        /// Initializes a new standard component type.
        /// </summary>
        /// <param name="collection">The collection the type belongs to.</param>
        /// <param name="item">The type of component within the collection.</param>
        public IOComponentType(string collection, string item)
        {
            Collection = collection;
            Item = item;
            GUID = Guid.Empty;
        }

        /// <summary>
        /// Determines whether two IOComponentTypes are equal.
        /// </summary>
        /// <param name="a">The first object.</param>
        /// <param name="b">The second object.</param>
        /// <returns>True if they are equal, false otherwise.</returns>
        public static bool operator ==(IOComponentType a, IOComponentType b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return (a.Collection == b.Collection &&
                a.Item == b.Item &&
                a.GUID == b.GUID &&
                a.Name == b.Name);
        }

        /// <summary>
        /// Determines whether two IOComponentTypes are not equal.
        /// </summary>
        /// <param name="a">The first object.</param>
        /// <param name="b">The second object.</param>
        /// <returns>True if they are not equal, false otherwise.</returns>
        public static bool operator !=(IOComponentType a, IOComponentType b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Determines whether this IOComponentType is equal to another object.
        /// </summary>
        /// <param name="obj">The o</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is IOComponentType))
                return false;

            return (this == obj as IOComponentType);
        }

        /// <summary>
        /// Returns the hash code for this IOComponentType.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            int collectionHashCode = (Collection != null ? Collection.GetHashCode() : 0);
            int itemHashCode = (Item != null ? Item.GetHashCode() : 0);
            int nameHashCode = (Name != null ? Name.GetHashCode() : 0);
            return collectionHashCode ^ itemHashCode ^ nameHashCode ^ IsStandard.GetHashCode() ^ GUID.GetHashCode();
        }
    }
}
