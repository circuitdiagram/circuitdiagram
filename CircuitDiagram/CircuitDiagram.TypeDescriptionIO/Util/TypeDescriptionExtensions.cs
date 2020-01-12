// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
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
using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.TypeDescriptionIO.Util
{
    public static class TypeDescriptionExtensions
    {
        public static IEnumerable<TypeDescriptionComponentType> GetComponentTypes(this ComponentDescription description)
        {
            var collection = !string.IsNullOrEmpty(description.Metadata.ImplementSet) ? new Uri(description.Metadata.ImplementSet) : ComponentType.UnknownCollection;
            var collectionItem = !string.IsNullOrEmpty(description.Metadata.ImplementItem) ? description.Metadata.ImplementItem : description.ComponentName;

            yield return new TypeDescriptionComponentType(description.Metadata.GUID, collection, collectionItem);

            foreach (var configuration in description.Metadata.Configurations.Where(x => x.ImplementationName != null))
            {
                yield return new TypeDescriptionComponentType(description.Metadata.GUID, collection, configuration.ImplementationName);
            }
        }

        public static TypeDescriptionComponentType GetComponentType(this ComponentDescription description)
        {
            return description.GetComponentTypes().First();
        }

        public static TypeDescriptionComponentType GetComponentType(this ComponentConfiguration configuration, ComponentDescription description)
        {
            var collection = !string.IsNullOrEmpty(description.Metadata.ImplementSet) ? new Uri(description.Metadata.ImplementSet) : ComponentType.UnknownCollection;
            var defaultCollectionItem = !string.IsNullOrEmpty(description.Metadata.ImplementItem) ? description.Metadata.ImplementItem : description.ComponentName;
            return new TypeDescriptionComponentType(description.Metadata.GUID, collection, string.IsNullOrEmpty(configuration.ImplementationName) ? defaultCollectionItem : configuration.ImplementationName);
        }

        public static TypeDescriptionComponentType GetConfigurationComponentType(this Component component, ComponentDescription description)
        {
            foreach (var configuration in description.Metadata.Configurations)
            {
                if (configuration.Matches(component))
                    return configuration.GetComponentType(description);
            }

            return description.GetComponentType();
        }
    }
}
