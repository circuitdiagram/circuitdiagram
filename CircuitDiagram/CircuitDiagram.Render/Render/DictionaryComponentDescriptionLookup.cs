using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.Render
{
    public class DictionaryComponentDescriptionLookup : IComponentDescriptionLookup
    {
        protected readonly Dictionary<ComponentType, ComponentDescription> LookupDictionary;
        private readonly Dictionary<Guid, ComponentDescription> guidLookup;

        public DictionaryComponentDescriptionLookup()
        {
            LookupDictionary = new Dictionary<ComponentType, ComponentDescription>();
            guidLookup = new Dictionary<Guid, ComponentDescription>();
        }

        public ComponentDescription GetDescription(ComponentType componentType)
        {
            var tdComponentType = componentType as  TypeDescriptionComponentType;
            ComponentDescription description;

            // 1. ID match
            if (tdComponentType?.Id != null && guidLookup.TryGetValue(tdComponentType.Id, out description))
                return description;

            // 2. ComponentType match
            if (LookupDictionary.TryGetValue(componentType, out description))
                return description;
            
            // Not found
            return null;
        }

        public void AddDescription(ComponentType componentType, ComponentDescription description)
        {
            if (!LookupDictionary.ContainsKey(componentType))
                LookupDictionary.Add(componentType, description);

            var tdComponentType = componentType as TypeDescriptionComponentType;

            if (tdComponentType?.Id != null)
                guidLookup[tdComponentType.Id] = description;
        }
    }
}
