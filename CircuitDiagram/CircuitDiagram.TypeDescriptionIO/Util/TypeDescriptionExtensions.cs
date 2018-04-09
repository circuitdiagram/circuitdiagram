using System;
using System.Collections.Generic;
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

            foreach (var configuration in description.Metadata.Configurations)
            {
                yield return new TypeDescriptionComponentType(description.Metadata.GUID, collection, configuration.ImplementationName);
            }
        }

        public static TypeDescriptionComponentType GetComponentType(this ComponentConfiguration configuration, ComponentDescription description)
        {
            var collection = !string.IsNullOrEmpty(description.Metadata.ImplementSet) ? new Uri(description.Metadata.ImplementSet) : ComponentType.UnknownCollection;
            var collectionItem = !string.IsNullOrEmpty(description.Metadata.ImplementItem) ? description.Metadata.ImplementItem : description.ComponentName;
            return new TypeDescriptionComponentType(description.Metadata.GUID, collection, configuration.ImplementationName ?? collectionItem ?? description.ComponentName);
        }
    }
}
